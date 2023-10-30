using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;


namespace UnityEngine.Rendering.Universal
{
    public class GPUStatsPrintRenderFeature : ScriptableRendererFeature
    {
        public enum QueryEventList
        {
            EVENT_CASCADE_SHADOW_PASS = 0,
            EVENT_PRE_PASS = 1,
            EVENT_OPAQUE_PASS = 2,
            EVENT_SKYBOX_PASS = 3,
            EVENT_TRANSPARENT_PASS = 4,
            EVENT_POSTPROCESS_PASS = 5,
            //TODO...Custom pass add here.




            EVENT_MAX_COUNT = 50,
        }

        private static int[] _counters = null;
        private static float[] _accumulated = null;
        private static float[] _maxtime = null;
        private static float[] _mintime = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport("RenderTimingPlugin")]
        private static extern void PrintLog(IntPtr ftp);
        [DllImport("RenderTimingPlugin")]
        private static extern void PrintLogError(IntPtr ftp);
        [DllImport("RenderTimingPlugin")]
        private static extern void PrintEventTime(IntPtr ftp);
        [DllImport("RenderTimingPlugin")]
        private static extern IntPtr TimeQueryBegin();
        [DllImport("RenderTimingPlugin")]
        private static extern IntPtr TimeQueryEnd();
        [DllImport("RenderTimingPlugin")]
        private static extern IntPtr TimeQueryEndFrame();

        /// True if rendering timing supported in this platform
        /// TODO: test whether GL extention is available

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PrintDelegate(string str);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PrintTimeDelegate(int eventID, float timeElapsed);

        [AOT.MonoPInvokeCallback(typeof(PrintDelegate))]
        static void LogCallBack(string str)
        {
            Debug.Log("GPUQueryPlugin LOG: " + str);
        }

        [AOT.MonoPInvokeCallback(typeof(PrintDelegate))]
        static void LogErrorCallBack(string str)
        {
            Debug.LogError("GPUQueryPlugin ERROR: " + str);
        }

        [AOT.MonoPInvokeCallback(typeof(PrintTimeDelegate))]
        static void EventTimingCallBack(int eventID, float timeElapsed)
        {
            QueryEventList eventEnum = (QueryEventList)eventID;
            if (timeElapsed > _maxtime[eventID]) {
                _maxtime[eventID] = timeElapsed;
            }
            if (timeElapsed < _mintime[eventID]) {
                _mintime[eventID] = timeElapsed;
            }
            _accumulated[eventID] += timeElapsed;
            _counters[eventID] += 1;
            String gpuTiming = String.Format("GPU Time: {0:F3} ms | Avg: {1:F3} ms | Max: {2:F3} ms | Min: {3:F3} ms | {4,-10}",
            timeElapsed,
            _accumulated[eventID] / (float)_counters[eventID],
            _maxtime[eventID],
            _mintime[eventID],
            eventEnum.ToString());
            GPUStatsPrintGUIDisplayer.Instance.UpdateGPUQueryInfo(eventID, gpuTiming);
            Debug.LogWarning("GPUQueryPlugin RESULT: " + gpuTiming);
        }

        //Custom pass query interface
        public static void beginTimeStampGPUQuery(CommandBuffer cmd, QueryEventList eventID) {
            if (cmd == null)
                return;
            if (eventID < QueryEventList.EVENT_MAX_COUNT) {
                cmd.IssuePluginEvent(TimeQueryBegin(), (int)eventID);
            }
        }
        public static void endTimeStampGPUQuery(CommandBuffer cmd, QueryEventList eventID) {
            if (cmd == null)
                return;
            if (eventID < QueryEventList.EVENT_MAX_COUNT)
            {
                cmd.IssuePluginEvent(TimeQueryEnd(), (int)eventID);
            }
        }
#else

        //Custom pass query interface
        public static void beginTimeStampGPUQuery(CommandBuffer cmd, QueryEventList eventID) {}
        public static void endTimeStampGPUQuery(CommandBuffer cmd, QueryEventList eventID) {}

#endif

        public override void Create()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            PrintDelegate log_callback_delegate = new PrintDelegate(LogCallBack);
            IntPtr intptr_delegate_log = Marshal.GetFunctionPointerForDelegate(log_callback_delegate);
            PrintLog(intptr_delegate_log);

            PrintDelegate log_error_callback_delegate = new PrintDelegate(LogErrorCallBack);
            IntPtr intptr_delegate_log_error = Marshal.GetFunctionPointerForDelegate(log_error_callback_delegate);
            PrintLogError(intptr_delegate_log_error);

            PrintTimeDelegate event_timing_callback_delegate = new PrintTimeDelegate(EventTimingCallBack);
            IntPtr intptr_delegate_event_timing = Marshal.GetFunctionPointerForDelegate(event_timing_callback_delegate);
            PrintEventTime(intptr_delegate_event_timing);

            if (_counters == null)
            {
                _counters = new int[(int)QueryEventList.EVENT_MAX_COUNT];
                for (int index = 0; index < (int)QueryEventList.EVENT_MAX_COUNT; ++index)
                    _counters[index] = 0;
            }
            if (_accumulated == null) 
            {
                _accumulated = new float[(int)QueryEventList.EVENT_MAX_COUNT];
                for (int index = 0; index < (int)QueryEventList.EVENT_MAX_COUNT; ++index)
                    _accumulated[index] = 0.0f;
            }
            if (_maxtime == null) 
            {
                _maxtime = new float[(int)QueryEventList.EVENT_MAX_COUNT];
                for (int index = 0; index < (int)QueryEventList.EVENT_MAX_COUNT; ++index)
                    _maxtime[index] = -1000000.0f;
            }
            if (_mintime == null)
            {
                _mintime = new float[(int)QueryEventList.EVENT_MAX_COUNT];
                for (int index = 0; index < (int)QueryEventList.EVENT_MAX_COUNT; ++index)
                    _mintime[index] = 1000000.0f;
            }
#endif
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_ANDROID && !UNITY_EDITOR

            //Shadow Pass
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.BeforeRenderingShadows, TimeQueryBegin(), "ShadowBeginQuery", QueryEventList.EVENT_CASCADE_SHADOW_PASS));
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.AfterRenderingShadows, TimeQueryEnd(), "ShadowEndQuery", QueryEventList.EVENT_CASCADE_SHADOW_PASS));
            //Pre pass 
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.BeforeRenderingPrePasses, TimeQueryBegin(), "PrepassBeginQuery", QueryEventList.EVENT_PRE_PASS));
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.AfterRenderingPrePasses, TimeQueryEnd(), "PrepassEndQuery", QueryEventList.EVENT_PRE_PASS));
            //Opaque Pass
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.BeforeRenderingOpaques, TimeQueryBegin(), "OpaqueBeginQuery", QueryEventList.EVENT_OPAQUE_PASS));
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.AfterRenderingOpaques, TimeQueryEnd(), "OpaqueEndQuery", QueryEventList.EVENT_OPAQUE_PASS));
            //SkyBox Pass
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.BeforeRenderingSkybox, TimeQueryBegin(), "SkyboxBeginQuery", QueryEventList.EVENT_SKYBOX_PASS));
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.AfterRenderingSkybox, TimeQueryEnd(), "SkyboxEndQuery", QueryEventList.EVENT_SKYBOX_PASS));
            //Transparent Pass
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.BeforeRenderingTransparents, TimeQueryBegin(), "TransparentBeginQuery", QueryEventList.EVENT_TRANSPARENT_PASS));
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.AfterRenderingTransparents, TimeQueryEnd(), "TransparentEndQuery", QueryEventList.EVENT_TRANSPARENT_PASS));
            //PostProcess Pass
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.BeforeRenderingPostProcessing, TimeQueryBegin(), "PostProcessBeginQuery", QueryEventList.EVENT_POSTPROCESS_PASS));
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.AfterRenderingPostProcessing, TimeQueryEnd(), "PostProcessEndQuery", QueryEventList.EVENT_POSTPROCESS_PASS));

            //End of frame
            renderer.EnqueuePass(new GPUStatsPrintRenderPass(RenderPassEvent.AfterRendering, TimeQueryEndFrame(), "QueryGPUTiming", 0/*usused*/));
#endif
        }
    }






}