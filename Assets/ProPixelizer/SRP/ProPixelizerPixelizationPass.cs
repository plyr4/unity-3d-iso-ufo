// Copyright Elliot Bentine, 2018-
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProPixelizer
{
    /// <summary>
    /// Performs the outline rendering and detection pass.
    /// </summary>
    public class PixelizationPass : ProPixelizerPass
    {
        public PixelizationPass(ShaderResources shaders)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            SourceBuffer = PixelizationSource.SceneColor;
            Materials = new MaterialLibrary(shaders);
        }

        private MaterialLibrary Materials;

        /// <summary>
        /// Shader resources used by the PixelizationPass.
        /// </summary>
        [Serializable]
        public sealed class ShaderResources
        {
            public Shader PixelizationMap;
            public Shader CopyDepth;
            public Shader CopyMainTexAndDepth;
            public Shader ApplyPixelizationMap;

            public ShaderResources Load()
            {
                PixelizationMap = Shader.Find(PixelizationMapShaderName);
                CopyDepth = Shader.Find(CopyDepthShaderName);
                ApplyPixelizationMap = Shader.Find(ApplyPixelizationMapShaderName);
                CopyMainTexAndDepth = Shader.Find(CopyMainTexAndDepthShaderName);
                return this;
            }
        }

        /// <summary>
        /// Materials used by the PixelizationPass
        /// </summary>
        public sealed class MaterialLibrary
        {
            private ShaderResources Resources;

            private Material _PixelizationMap;
            public Material PixelizationMap
            {
                get
                {
                    if (_PixelizationMap == null)
                        _PixelizationMap = new Material(Resources.PixelizationMap);
                    return _PixelizationMap;
                }
            }
            private Material _CopyDepth;
            public Material CopyDepth
            {
                get
                {
                    if (_CopyDepth == null)
                        _CopyDepth = new Material(Resources.CopyDepth);
                    return _CopyDepth;
                }
            }
            private Material _CopyMainTexAndDepth;
            public Material CopyMainTexAndDepth
            {
                get
                {
                    if (_CopyMainTexAndDepth == null)
                        _CopyMainTexAndDepth = new Material(Resources.CopyMainTexAndDepth);
                    return _CopyMainTexAndDepth;
                }
            }
            private Material _ApplyPixelizationMap;
            public Material ApplyPixelizationMap
            {
                get
                {
                    if (_ApplyPixelizationMap == null)
                        _ApplyPixelizationMap = new Material(Resources.ApplyPixelizationMap);
                    return _ApplyPixelizationMap;
                }
            }

            public MaterialLibrary(ShaderResources resources)
            {
                Resources = resources;
            }
        }

        private int _PixelizationMap;
        private int _OriginalScene;
        private int _CameraColorTexture;
        private int _PixelatedScene;
        private int _CameraDepthAttachment;
        private int _CameraDepthAttachmentTemp;
        private int _CameraDepthTexture;
        private int _ProPixelizerOutline;
        private int _ProPixelizerOutlineObject;

        private const string CopyDepthShaderName = "Hidden/ProPixelizer/SRP/BlitCopyDepth";
        private const string CopyMainTexAndDepthShaderName = "Hidden/ProPixelizer/SRP/BlitCopyMainTexAndDepth";
        private const string PixelizationMapShaderName = "Hidden/ProPixelizer/SRP/Pixelization Map";
        private const string ApplyPixelizationMapShaderName = "Hidden/ProPixelizer/SRP/ApplyPixelizationMap";

        private Vector4 TexelSize;

        public enum PixelizationSource
        {
            SceneColor,
            ProPixelizerMetadata
        }

        public PixelizationSource SourceBuffer;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            _PixelizationMap = Shader.PropertyToID("_PixelizationMap");
            _CameraColorTexture = Shader.PropertyToID("_CameraColorTexture");
            _PixelatedScene = Shader.PropertyToID("_PixelatedScene");
            _OriginalScene = Shader.PropertyToID("_OriginalScene");
            _ProPixelizerOutline = Shader.PropertyToID(OutlineDetectionPass.OUTLINE_BUFFER);
            _ProPixelizerOutlineObject = Shader.PropertyToID(OutlineDetectionPass.PROPIXELIZER_OBJECT_BUFFER);

            cameraTextureDescriptor.useMipMap = false;
            
            // Issue #3
            //cmd.GetTemporaryRT(_CameraColorTexture, cameraTextureDescriptor);
            cmd.GetTemporaryRT(_PixelatedScene, cameraTextureDescriptor);

            cmd.GetTemporaryRT(_OriginalScene, cameraTextureDescriptor, FilterMode.Point);

            _CameraDepthAttachment = Shader.PropertyToID("_CameraDepthAttachment");
            _CameraDepthAttachmentTemp = Shader.PropertyToID("_CameraDepthAttachmentTemp");
            _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
            var depthDescriptor = cameraTextureDescriptor;
            depthDescriptor.colorFormat = RenderTextureFormat.Depth;
            cmd.GetTemporaryRT(_CameraDepthAttachment, depthDescriptor);
            cmd.GetTemporaryRT(_CameraDepthAttachmentTemp, depthDescriptor);

            var pixelizationMapDescriptor = cameraTextureDescriptor;
            pixelizationMapDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            pixelizationMapDescriptor.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            cmd.GetTemporaryRT(_PixelizationMap, pixelizationMapDescriptor);
            
            TexelSize = new Vector4(
                1f / cameraTextureDescriptor.width,
                1f / cameraTextureDescriptor.height,
                cameraTextureDescriptor.width,
                cameraTextureDescriptor.height
            );
        }

        public const string PROFILER_TAG = "PIXELISATION";

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer buffer = CommandBufferPool.Get(PROFILER_TAG);
            buffer.name = "Pixelisation";

            // Configure keywords for pixelising material.
            if (renderingData.cameraData.camera.orthographic)
            {
                Materials.PixelizationMap.EnableKeyword("ORTHO_PROJECTION");
            }
            else
            {
                Materials.PixelizationMap.DisableKeyword("ORTHO_PROJECTION");
            }

            if (renderingData.cameraData.camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay)
                Materials.PixelizationMap.EnableKeyword("OVERLAY_CAMERA"); 
            else
                Materials.PixelizationMap.DisableKeyword("OVERLAY_CAMERA");

#if CAMERA_COLOR_TEX_PROP
            RenderTargetIdentifier ColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            bool applyFeature = true;
#else
            // Preview camera does not use _CameraColorTexture target in more recent 2019 LTS.
            // It now uses a temporary buffer, and API does not provide a means to get the ID.
            int ColorTarget = _CameraColorTexture;
            bool applyFeature = !renderingData.cameraData.isPreviewCamera;
#endif

            if (!applyFeature)
            {
                context.ExecuteCommandBuffer(buffer);
                CommandBufferPool.Release(buffer);
                return;
            }

            // Blit scene into _OriginalScene - so that we can guarantee point filtering of colors.
            Blit(buffer, ColorTarget, _OriginalScene);

#if CAMERA_COLOR_TEX_PROP
            bool isOverlay = renderingData.cameraData.camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay;
#else
            bool isOverlay = false;
#endif

            // Create pixelization map, to determine how to pixelate the screen.
            if (SourceBuffer == PixelizationSource.SceneColor)
            {
                buffer.SetGlobalTexture("_MainTex", _OriginalScene);
#if CAMERA_COLOR_TEX_PROP
                // Overlay cameras do not have a _CameraDepthTexture assigned, so our first action is to write
                // the opaque depth buffer into _CameraDepthTexture.
                if (isOverlay)
                {
                    buffer.SetGlobalTexture("_SourceDepthTexture", renderingData.cameraData.renderer.cameraDepthTarget);
                    buffer.SetGlobalTexture("_SceneDepthTexture", renderingData.cameraData.renderer.cameraDepthTarget);
                }
                else
                {
                    buffer.SetGlobalTexture("_SourceDepthTexture", _CameraDepthTexture);
                    buffer.SetGlobalTexture("_SceneDepthTexture", _CameraDepthTexture);
                }
#else
                buffer.SetGlobalTexture("_SourceDepthTexture", _CameraDepthTexture);
                buffer.SetGlobalTexture("_SceneDepthTexture", _CameraDepthTexture);
#endif
            } else {
                buffer.SetGlobalTexture("_MainTex", _ProPixelizerOutlineObject);
                buffer.SetGlobalTexture("_SourceDepthTexture", _ProPixelizerOutlineObject, RenderTextureSubElement.Depth);
                if (renderingData.cameraData.camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay)
                {
                    buffer.SetGlobalTexture("_SceneDepthTexture", renderingData.cameraData.renderer.cameraDepthTarget);
                }
                else
                {
                    buffer.SetGlobalTexture("_SceneDepthTexture", _CameraDepthTexture);
                }
            }
            Blit(buffer, _OriginalScene, _PixelizationMap, Materials.PixelizationMap);

            // Pixelise the appearance texture
            buffer.SetGlobalTexture("_MainTex", _OriginalScene);
            buffer.SetGlobalTexture("_PixelizationMap", _PixelizationMap);
            Blit(buffer, ColorTarget, _PixelatedScene, Materials.ApplyPixelizationMap);

            // Copy depth into Color target so that transparents work in scene view tab (which uses a depth RenderTextureSubElement of the color target).
            buffer.SetRenderTarget(ColorTarget);
            buffer.SetGlobalTexture("_MainTex", _PixelatedScene);
            buffer.SetGlobalTexture("_SourceTex", _PixelatedScene); // so that it works for fallback pass for platforms that fail to compile BCMT&D
            buffer.SetGlobalTexture("_Depth", _PixelatedScene, RenderTextureSubElement.Depth);
            buffer.SetViewMatrix(Matrix4x4.identity);
            buffer.SetProjectionMatrix(Matrix4x4.identity);
            buffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, Materials.CopyMainTexAndDepth);

            // Copy pixelated depth texture
            buffer.SetGlobalTexture("_MainTex", _PixelatedScene, RenderTextureSubElement.Depth);
            buffer.SetRenderTarget(_CameraDepthAttachmentTemp);
            buffer.SetViewMatrix(Matrix4x4.identity);
            buffer.SetProjectionMatrix(Matrix4x4.identity);
            buffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, Materials.CopyDepth);

            // ...then restore transformations:
#if CAMERADATA_MATRICES
            buffer.SetViewMatrix(renderingData.cameraData.GetViewMatrix());
            buffer.SetProjectionMatrix(renderingData.cameraData.GetProjectionMatrix());
#else
            buffer.SetViewMatrix(renderingData.cameraData.camera.worldToCameraMatrix);
            buffer.SetProjectionMatrix(renderingData.cameraData.camera.projectionMatrix);
#endif

            //// Blit pixelised depth into the used depth texture
            if (!isOverlay)
                Blit(buffer, _CameraDepthAttachmentTemp, _CameraDepthTexture, Materials.CopyDepth);
            Blit(buffer, _CameraDepthAttachmentTemp, _CameraDepthAttachment, Materials.CopyDepth);

            if (!isOverlay)
                buffer.SetGlobalTexture("_CameraDepthTexture", _CameraDepthTexture);

            // ...and restore transformations:
#if CAMERADATA_MATRICES
            buffer.SetViewMatrix(renderingData.cameraData.GetViewMatrix());
            buffer.SetProjectionMatrix(renderingData.cameraData.GetProjectionMatrix());
#else
            buffer.SetViewMatrix(renderingData.cameraData.camera.worldToCameraMatrix);
            buffer.SetProjectionMatrix(renderingData.cameraData.camera.projectionMatrix);
#endif
            context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_PixelizationMap);
            // Issue #3
            //cmd.ReleaseTemporaryRT(_CameraColorTexture);
            cmd.ReleaseTemporaryRT(_CameraDepthAttachment);
            cmd.ReleaseTemporaryRT(_CameraDepthAttachmentTemp);
            // Don't release camera depth - causes bugs.
            //cmd.ReleaseTemporaryRT(_CameraDepthTexture);
            cmd.ReleaseTemporaryRT(_PixelatedScene);
            cmd.ReleaseTemporaryRT(_OriginalScene);
        }
    }
}