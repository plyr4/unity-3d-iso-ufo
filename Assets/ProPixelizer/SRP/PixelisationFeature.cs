﻿// Copyright Elliot Bentine, 2018-
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using ProPixelizer;

public class PixelisationFeature : ScriptableRendererFeature
{
    [FormerlySerializedAs("DepthTestOutlines")]
    [Tooltip("Perform depth testing for outlines where object IDs differ. This prevents outlines appearing when one object intersects another, but requires an extra depth sample.")]
    public bool UseDepthTestingForIDOutlines = true;

    [Tooltip("The threshold value used when depth comparing outlines.")]
    public float DepthTestThreshold = 0.001f;

    [Tooltip("Use normals for edge detection. This will analyse pixelated screen normals to determine where edges occur within an objects silhouette.")]
    public bool UseNormalsForEdgeDetection = true;

    public float NormalEdgeDetectionSensitivity = 1f;

    [HideInInspector, SerializeField]
    PixelizationPass.ShaderResources PixelizationShaders;
    [HideInInspector, SerializeField]
    OutlineDetectionPass.ShaderResources OutlineShaders;

    PixelizationPass _PixelisationPass;
    OutlineDetectionPass _OutlinePass;

    public override void Create()
    {
        PixelizationShaders = new PixelizationPass.ShaderResources().Load();
        _PixelisationPass = new PixelizationPass(PixelizationShaders);
        _PixelisationPass.SourceBuffer = PixelizationPass.PixelizationSource.ProPixelizerMetadata;
        OutlineShaders = new OutlineDetectionPass.ShaderResources().Load();
        _OutlinePass = new OutlineDetectionPass(OutlineShaders);
        _OutlinePass.DepthTestOutlines = UseDepthTestingForIDOutlines;
        _OutlinePass.DepthTestThreshold = DepthTestThreshold;
        _OutlinePass.UseNormalsForEdgeDetection = UseNormalsForEdgeDetection;
        _OutlinePass.NormalEdgeDetectionSensitivity = NormalEdgeDetectionSensitivity;
        ProPixelizerVerification.Check();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_PixelisationPass);
        renderer.EnqueuePass(_OutlinePass);
    }
}