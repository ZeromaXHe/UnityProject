using System;
using UnityEngine;

namespace CatlikeCodings.Basics
{
    public class GpuGraph : MonoBehaviour
    {
        private const int maxResolution = 1000;
        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private Material material;
        [SerializeField] private Mesh mesh;

        [SerializeField, Range(10, maxResolution)]
        private int resolution = 10;

        [SerializeField] private FunctionLibrary.FunctionName function;

        private enum TransitionMode
        {
            Cycle,
            Random
        }

        [SerializeField] private TransitionMode transitionMode;
        [SerializeField, Min(0f)] private float functionDuration = 1f, transitionDuration = 1f;

        private float _duration;
        private bool _transitioning;
        private FunctionLibrary.FunctionName _transitionFunction;
        private ComputeBuffer _positionsBuffer;

        private void OnEnable()
        {
            _positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
        }

        private void OnDisable()
        {
            _positionsBuffer.Release();
            _positionsBuffer = null;
        }

        // Update is called once per frame
        private void Update()
        {
            _duration += Time.deltaTime;
            if (_transitioning)
            {
                if (_duration >= transitionDuration)
                {
                    _duration -= transitionDuration;
                    _transitioning = false;
                }
            }
            else if (_duration >= functionDuration)
            {
                _duration -= functionDuration;
                _transitioning = true;
                _transitionFunction = function;
                PickNextFunction();
            }

            UpdateFunctionOnGPU();
        }

        private void PickNextFunction()
        {
            function = transitionMode == TransitionMode.Cycle
                ? FunctionLibrary.GetNextFunctionName(function)
                : FunctionLibrary.GetRandomFunctionNameOtherThan(function);
        }

        private static readonly int
            PositionsId = Shader.PropertyToID("_Positions"),
            ResolutionId = Shader.PropertyToID("_Resolution"),
            StepId = Shader.PropertyToID("_Step"),
            TimeId = Shader.PropertyToID("_Time"),
            TransitionProgressId = Shader.PropertyToID("_TransitionProgress");

        private void UpdateFunctionOnGPU()
        {
            var step = 2f / resolution;
            computeShader.SetInt(ResolutionId, resolution);
            computeShader.SetFloat(StepId, step);
            computeShader.SetFloat(TimeId, Time.time);
            if (_transitioning)
            {
                computeShader.SetFloat(
                    TransitionProgressId,
                    Mathf.SmoothStep(0f, 1f, _duration / transitionDuration)
                );
            }

            var kernelIndex = (int)function +
                              (int)(_transitioning ? _transitionFunction : function) * FunctionLibrary.FunctionCount;
            computeShader.SetBuffer(kernelIndex, PositionsId, _positionsBuffer);

            var groups = Mathf.CeilToInt(resolution / 8f);
            computeShader.Dispatch(kernelIndex, groups, groups, 1);
            material.SetBuffer(PositionsId, _positionsBuffer);
            material.SetFloat(StepId, step);
            var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, resolution * resolution
            );
        }
    }
}