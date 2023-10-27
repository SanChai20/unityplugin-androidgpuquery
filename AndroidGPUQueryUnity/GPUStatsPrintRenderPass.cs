
using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    public class GPUStatsPrintRenderPass : ScriptableRenderPass
    {
        private IntPtr _pluginCallback;
        private string _eventName;
        private GPUStatsPrintRenderFeature.QueryEventList _eventID;
        public GPUStatsPrintRenderPass(RenderPassEvent renderPassEvent, IntPtr pluginCallBack, string eventName, GPUStatsPrintRenderFeature.QueryEventList eventID)
        {
            this.renderPassEvent = renderPassEvent;
            this._pluginCallback = pluginCallBack;
            this._eventName = eventName;
            this._eventID = eventID;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(this._eventName)))
            {
                cmd.name = this._eventName;
                cmd.IssuePluginEvent(this._pluginCallback, (int)this._eventID);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

}