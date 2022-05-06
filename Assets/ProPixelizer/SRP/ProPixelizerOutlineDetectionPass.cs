﻿// Copyright Elliot Bentine, 2018-
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProPixelizer
{
    /// <summary>
    /// Performs the outline rendering and detection pass.
    /// </summary>
    public class OutlineDetectionPass : ProPixelizerPass
    {
        public OutlineDetectionPass(ShaderResources resources)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            Materials = new MaterialLibrary(resources);
        }

        private MaterialLibrary Materials;

        /// <summary>
        /// Shader resources used by the OutlineDetectionPass.
        /// </summary>
        [Serializable]
        public sealed class ShaderResources
        {
            public Shader OutlineDetection;

            public ShaderResources Load()
            {
                OutlineDetection = Shader.Find(OutlineDetectionShader);
                return this;
            }
        }

        /// <summary>
        /// Materials used by the OutlineDetectionPass.
        /// </summary>
        public sealed class MaterialLibrary
        {
            private ShaderResources Resources;
            public Material OutlineDetection
            {
                get
                {
                    if (_OutlineDetection == null)
                        _OutlineDetection = new Material(Resources.OutlineDetection);
                    return _OutlineDetection;
                }
            }
            private Material _OutlineDetection;

            public MaterialLibrary(ShaderResources resources)
            {
                Resources = resources;
            }
        }


        public bool DepthTestOutlines;
        public float DepthTestThreshold;
        public bool UseNormalsForEdgeDetection = true;
        public float NormalEdgeDetectionSensitivity = 1f;

        /// <summary>
        /// Buffer into which objects are rendered using the Outline pass.
        /// </summary>
        private int _OutlineObjectBuffer;

        /// <summary>
        /// Buffer to store the results from outline analysis.
        /// </summary>
        private int _OutlineBuffer;

        static ShaderTagId ProPixelizerShaderTagID = new ShaderTagId(PROPIXELIZER_SHADER_TAG);

        private const string OutlineDetectionShader = "Hidden/ProPixelizer/SRP/OutlineDetection";

        private Vector4 TexelSize;
        public const string PROPIXELIZER_OBJECT_BUFFER = "_ProPixelizerMetadata";
        public const string OUTLINE_BUFFER = "_ProPixelizerOutlines";
        private const string PROPIXELIZER_SHADER_TAG = "ProPixelizer";

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var outlineDescriptor = cameraTextureDescriptor;
            outlineDescriptor.useMipMap = false;
            outlineDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            outlineDescriptor.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            _OutlineObjectBuffer = Shader.PropertyToID(PROPIXELIZER_OBJECT_BUFFER);
            _OutlineBuffer = Shader.PropertyToID(OUTLINE_BUFFER);
            cmd.GetTemporaryRT(_OutlineObjectBuffer, outlineDescriptor, FilterMode.Point);
            cmd.GetTemporaryRT(_OutlineBuffer, outlineDescriptor, FilterMode.Point);

            TexelSize = new Vector4(
                1f / cameraTextureDescriptor.width,
                1f / cameraTextureDescriptor.height,
                cameraTextureDescriptor.width,
                cameraTextureDescriptor.height
            );
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_OutlineObjectBuffer);
            cmd.ReleaseTemporaryRT(_OutlineBuffer);
        }

        public const string PROFILER_TAG = "ProPixelizerOutlines";

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (DepthTestOutlines)
            {
                Materials.OutlineDetection.EnableKeyword("DEPTH_TEST_OUTLINES_ON");
                Materials.OutlineDetection.SetFloat("_OutlineDepthTestThreshold", DepthTestThreshold);
            }
            else
                Materials.OutlineDetection.DisableKeyword("DEPTH_TEST_OUTLINES_ON");

            if (UseNormalsForEdgeDetection)
            {
                //OutlineDetectionMaterial.EnableKeyword("NORMAL_EDGE_DETECTION_ON");
                Materials.OutlineDetection.SetFloat("_NormalEdgeDetectionSensitivity", NormalEdgeDetectionSensitivity);
            }
            //else
            //    OutlineDetectionMaterial.DisableKeyword("NORMAL_EDGE_DETECTION_ON");

            CommandBuffer buffer = CommandBufferPool.Get(PROFILER_TAG);
            buffer.name = "ProPixelizer Outline Pass";
            
            if (UseNormalsForEdgeDetection)
                buffer.EnableShaderKeyword("NORMAL_EDGE_DETECTION_ON");
            else
                buffer.DisableShaderKeyword("NORMAL_EDGE_DETECTION_ON");


            // Set up matrices for rendering outlines.
#if CAMERADATA_MATRICES
            buffer.SetViewMatrix(renderingData.cameraData.GetViewMatrix());
            buffer.SetProjectionMatrix(renderingData.cameraData.GetProjectionMatrix());
#else
            buffer.SetViewMatrix(renderingData.cameraData.camera.worldToCameraMatrix);
            buffer.SetProjectionMatrix(renderingData.cameraData.camera.projectionMatrix);
#endif

            // Render outlines into a render target.
            buffer.SetRenderTarget(_OutlineObjectBuffer);
            buffer.ClearRenderTarget(true, true, Color.white);
            context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);

            var sort = new SortingSettings(renderingData.cameraData.camera);
            var drawingSettings = new DrawingSettings(ProPixelizerShaderTagID, sort);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            // Perform outline detection
            buffer = CommandBufferPool.Get(PROFILER_TAG);
            buffer.name = "ProPixelizer Outline Detection";
            buffer.SetGlobalTexture("_MainTex", _OutlineObjectBuffer);
            buffer.SetGlobalTexture("_MainTex_Depth", _OutlineObjectBuffer, RenderTextureSubElement.Depth);
            buffer.SetGlobalVector("_TexelSize", TexelSize);
            Blit(buffer, _OutlineObjectBuffer, _OutlineBuffer, Materials.OutlineDetection);
            buffer.SetGlobalTexture(OUTLINE_BUFFER, _OutlineBuffer);
            context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }
    }
}