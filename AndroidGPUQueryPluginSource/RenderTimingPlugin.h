/*
 * Copyright 2017 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#pragma once
#include "Unity/IUnityInterface.h"

//Platform
#if _MSC_VER
#define UNITY_WIN 1
#elif defined(__APPLE__)
  #if defined(__arm__)
    #define UNITY_IPHONE 1
  #else
    #define UNITY_OSX 1
  #endif
#elif defined(UNITY_METRO) || defined(UNITY_ANDROID) || defined(UNITY_LINUX)

#else
#error "Unknown platform!"
#endif

//Graphic API
#if UNITY_METRO
  #define SUPPORT_D3D11 1
  #if WINDOWS_UWP
    #define SUPPORT_D3D12 1
  #endif
#elif UNITY_WIN
  #define SUPPORT_D3D9 1
  #define SUPPORT_D3D11 1 
  #ifdef _MSC_VER
    #if _MSC_VER >= 1900
      #define SUPPORT_D3D12 1
    #endif
  #endif
  #define SUPPORT_OPENGL_LEGACY 1
  #define SUPPORT_OPENGL_UNIFIED 1
  #define SUPPORT_OPENGL_CORE 1
#elif UNITY_IPHONE || UNITY_ANDROID
  #define SUPPORT_OPENGL_UNIFIED 1
  #define SUPPORT_OPENGL_ES 1
#elif UNITY_OSX || UNITY_LINUX
  #define SUPPORT_OPENGL_LEGACY 1
  #define SUPPORT_OPENGL_UNIFIED 1
  #define SUPPORT_OPENGL_CORE 1
#endif

// #if SUPPORT_OPENGL_UNIFIED
//     #if UNITY_IPHONE
//         #include <OpenGLES/ES2/gl.h>
//     #elif UNITY_ANDROID
//         #include <GLES3/gl3.h>
//         #define GL_TIME_ELAPSED                   0x88BF
//         #define GL_GPU_DISJOINT                   0x8FBB
//     #else
//         #include "GL/glew.h"
//     #endif

// // Define Query Data Class
// class GLTimeStampGPUQuery 
// {
// public:
//     static const int FRAME_COUNT = 2;
//     static const int QUERY_COUNT = 50;//call record query function up to 50 times within one frame.
// private:
//     GLuint _query[QUERY_COUNT * FRAME_COUNT];//2 stands for frame count. 50 stands for query count
//     int _frameCount = 0;
// public:
//     void init();
//     GLuint queryForWrite(int eventID);
//     GLuint queryForRead(int eventID);
//     void newFrame();
// };
// #include <vector>
// class GLTimeStampGPUQueryWrapper 
// {
// private: 
//     GLTimeStampGPUQuery _timeQueryData;
//     std::vector<int> _eventIDList;
// public:
//     void init();
//     void begin(int eventID);
//     void end(int eventID);
//     void endFrame();
// };
// #endif

