using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    #region 局部变量
    private static T _Instance;
    #endregion
    #region 属性
    /// <summary>
    /// 获取单例对象
    /// </summary>
    public static T Instance
    {
        get
        {
            if (null == _Instance)
            {
                _Instance = FindObjectOfType<T>();
                if (null == _Instance)
                {
                    GameObject go = new GameObject();
                    go.name = typeof(T).Name;
                    _Instance = go.AddComponent<T>();
                }
            }
            return _Instance;
        }
    }
    #endregion
    #region 方法
    protected virtual void Awake()
    {
        if (null == _Instance)
        {
            _Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
public class GPUStatsPrintGUIDisplayer : Singleton<GPUStatsPrintGUIDisplayer>
{
    private Dictionary<int, string> _queryTimingDict = new Dictionary<int, string>();//key : int - offset . value : string - gpu time + event name
    private List<int> _keylist;
    private GUIStyle _backGroundStyle;
    private GUIContent _groupContent;
    private Rect _groupRect;
    private static int TOP_OFFSET = 15;
    private static int LEFT_OFFSET = 100;
    private static int FONT_SIZE = 20;

    private UniversalRendererData _urpData;
    private GPUStatsPrintRenderFeature _gpuQueryFeature;

    private GPUStatsPrintGUIDisplayer() {}

    private void Start()
    {
        Debug.LogWarning("GPUStatsPrintGUIDisplayer Start...");
        UniversalRenderPipelineAsset URPAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
        FieldInfo propertyInfo = URPAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        _urpData = (UniversalRendererData)(((ScriptableRendererData[])propertyInfo?.GetValue(URPAsset))?[0]);
        _gpuQueryFeature = ScriptableObject.CreateInstance<GPUStatsPrintRenderFeature>();
        if (_urpData != null && _gpuQueryFeature != null)
        {
            _urpData.rendererFeatures.Add(_gpuQueryFeature);
            _urpData.SetDirty();
        }
    }

    private void OnDestroy()
    {
        Debug.LogWarning("GPUStatsPrintGUIDisplayer OnDestroy...");
        if (_urpData != null && _gpuQueryFeature != null) 
            _urpData.rendererFeatures.Remove(_gpuQueryFeature);
    }

    public void UpdateGPUQueryInfo(int offset, string eventTiming)
    {
        _queryTimingDict[offset] = eventTiming;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    void OnGUI()
    {
        if (_backGroundStyle == null) {
            _backGroundStyle = new GUIStyle();
            _backGroundStyle.normal.background = MakeTex(1, 1, new Color(0f, 0f, 0f, 0.5f));
            _backGroundStyle.fontSize = FONT_SIZE;
            _backGroundStyle.alignment = TextAnchor.MiddleLeft;
            _backGroundStyle.padding = new RectOffset(10, 10, 0, 0);
            _backGroundStyle.normal.textColor = new Color(1, 1, 1);
        }
        if (_groupContent == null) {
            _groupContent = new GUIContent();
        }
        if (_groupRect == null) {
            _groupRect = new Rect(LEFT_OFFSET, TOP_OFFSET, 0, 0);
        }
        _keylist = new List<int>(_queryTimingDict.Keys);
        if (_keylist.Count != 0) 
        {
            string textResult = "";
            for (int index = 0; index < _keylist.Count; ++index)
            {
                int offset = _keylist[index];
                string gpuTiming = _queryTimingDict[offset];
                textResult += "\n";
                textResult += gpuTiming;
                textResult += "\n";
            }
            _groupContent.text = textResult;
            _groupRect.size = _backGroundStyle.CalcSize(_groupContent);
            GUI.BeginGroup(_groupRect, _groupContent, _backGroundStyle);
            GUI.EndGroup();

            var e = Event.current;
            if (e.type == EventType.MouseDrag && _groupRect.Contains(e.mousePosition))
            {
                _groupRect.x += e.delta.x;
#if UNITY_ANDROID && !UNITY_EDITOR
                _groupRect.y -= e.delta.y;
#else
                _groupRect.y += e.delta.y;
#endif
                if (_groupRect.width < Screen.width && _groupRect.height < Screen.height) 
                {
                    if (_groupRect.x < 0)
                    {
                        _groupRect.x = 0;
                    }
                    if (_groupRect.y < 0)
                    {
                        _groupRect.y = 0;
                    }
                    if (_groupRect.x + _groupRect.width > Screen.width)
                    {
                        _groupRect.x = Screen.width - _groupRect.width;
                    }
                    if (_groupRect.y + _groupRect.height > Screen.height)
                    {
                        _groupRect.y = Screen.height - _groupRect.height;
                    }
                }
            }
        }
    }

}
