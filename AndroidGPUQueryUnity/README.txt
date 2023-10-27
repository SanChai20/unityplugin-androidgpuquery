Tutotial

if you just want to monitor urp passes gpu timing, drag GPUTimingPrinter prefab which is under Asset/Plugins/GPUQueryPluginRuntime path into scene directly.
if you want to monitor custom render feature either, GPUStatsPrintRenderFeature.beginTimeStampGPUQuery function should be called in front of draw commands during execute function and GPUStatsPrintRenderFeature.endTimeStampGPUQuery function should be called after draw commands. And add corresponding enum in GPUStatsPrintRenderFeature.QueryEventList.

如果只想监测URP前向渲染管线中的内置Pass(如ShadowPass/OpaquePass/TransparentPass/PostprocessPass等)的GPU耗时，那么直接将Asset/Plugins/GPUQueryPluginRuntime路径下的GPUTimingPrinter预制件拖拽到场景中即可，注意需要打OpenGLES包，目前暂不支持Vulkan
如果想检测URP管线中其它自定义添加的RenderFeature的GPU耗时，除去以上步骤，还需要在Execute函数中CommandBuffer录入任何绘制指令(DrawMesh/Blit等)前调用GPUStatsPrintRenderFeature.beginTimeStampGPUQuery，在录入绘制指令后调用GPUStatsPrintRenderFeature.endTimeStampGPUQuery，并且需要在GPUStatsPrintRenderFeature.QueryEventList添加自定义的Pass事件枚举
