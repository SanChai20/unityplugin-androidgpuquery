Tutotial

if you want to monitor gpu timings of urp internal passes, just drag GPUTimingPrinter prefab which is under Asset/Plugins/GPUQueryPluginRuntime path into scene directly.

if you want to monitor custom render feature either, GPUStatsPrintRenderFeature.beginTimeStampGPUQuery function should be called in front of draw commands within execute function and GPUStatsPrintRenderFeature.endTimeStampGPUQuery function should be called after draw commands. And add corresponding enum in GPUStatsPrintRenderFeature.QueryEventList.
