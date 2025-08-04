using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


/// <summary>
/// Text组件的扩展 绘制text下划线
/// </summary>
[RequireComponent(typeof(Text)), DisallowMultipleComponent]
public class TextLineExpand : BaseMeshEffect
{
    //面板属性
    [Header("下划线")] public bool UnderLine = true;

    //[Header("删除线")]
    private bool Strickout = false;

    [Header("下划线宽度"), Range(0f, 100f), SerializeField]
    private float _lineHeight = 3.0f;

    public float lineHeight
    {
        get { return _lineHeight; }
        set { _lineHeight = value; }
    }

    [Header("下划线颜色")] public Color32 lineColor = Color.white;

    //字段
    private Text text;
    private UICharInfo[] characters;
    private UILineInfo[] lines;
    private char[] textChars;
    private List<UIVertex> stream = null;
    private int characterCountVisible = 0; //可视字符个数

    protected override void Awake()
    {
        text = this.GetComponent<Text>();
        if (text == null) return;
        text.RegisterDirtyMaterialCallback(OnFontMaterialChanged);
        lineColor = text.color;
    }
#if UNITY_EDITOR
        protected override void OnEnable()
        {
            text = GetComponent<Text>();
            text.RegisterDirtyMaterialCallback(OnFontMaterialChanged);
        }
#endif
    private void OnFontMaterialChanged()
    {
        //font纹理发生变化时,在font中注册一个字符
        text.font.RequestCharactersInTexture("*", text.fontSize, text.fontStyle);
    }

    protected override void OnDestroy()
    {
        text.UnregisterDirtyMaterialCallback(OnFontMaterialChanged);
        base.OnDestroy();
    }

    /// <summary>
    /// 从font纹理中获取指定字符的uv
    /// </summary>
    private Vector2 GetUnderlineCharUV()
    {
        CharacterInfo info;
        if (text.font.GetCharacterInfo('*', out info, text.fontSize, text.fontStyle))
        {
            return (info.uvBottomLeft + info.uvBottomRight + info.uvTopLeft + info.uvTopRight) * 0.25f;
        }

        Debug.LogWarning("GetCharacterInfo failed");
        return Vector2.zero;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || (!UnderLine && !Strickout))
        {
            return;
        }

        //当宽或高足够小则不处理
        if (text.rectTransform.rect.size.x <= 0 || text.rectTransform.rect.size.y <= 0) return;

        //cachedTextGenerator是当前实际显示出来的相关信息,cachedTextGeneratorForLayout是所有布局信息(包括看不到的)
        characters = text.cachedTextGenerator.GetCharactersArray();
        lines = text.cachedTextGenerator.GetLinesArray();
        if (UnderLine || Strickout)
        {
            textChars = text.text.ToCharArray();
        }

        //使用characterCountVisible来得到真正显示的字符数量.characterCount会额外包含(在宽度不足)时候裁剪掉的(边缘)字符,会导致显示的下划线多一个空白的宽度
        characterCountVisible = text.cachedTextGenerator.characterCountVisible;

        /* stream是6个一组(对应三角形索引信息,左上角开始,顺时针)对应1个字(含空白)
            0-1
             \|
              2
            0
            |\
            3-2
            数组索引:
            [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, ...]
            对应三角形索引:
            [0, 1, 2, 2, 3, 0, 4, 5, 6, 6,  7,  4,...]
        */
        stream = new List<UIVertex>();

        vh.GetUIVertexStream(stream);
        vh.Clear();

        //添加下划线
        if (UnderLine || Strickout)
        {
            //if (!useCustomLineIndexArray)
            //{
            //    DrawAllLinesLine(vh);
            //}
            //else
            //{
            //    DrawCustomLine(vh);
            //}
            DrawAllLinesLine(vh);
        }

        vh.AddUIVertexTriangleStream(stream);
    }

    /// <summary>
    /// 获取一个字符索引所在的行
    /// </summary>
    private int GetCharInLineIndex(int charIndex)
    {
        var ei = this.lines.Length - 1;
        for (int i = 0; i < ei; i++)
        {
            var line = lines[i];
            if (charIndex >= line.startCharIdx && charIndex < lines[i + 1].startCharIdx) return i;
        }

        // 是否在最后一行
        if (charIndex >= lines[ei].startCharIdx && charIndex < this.characters.Length) return ei;

        return -1;
    }

    /// <summary>
    /// 绘制下划线
    /// <param name="vh"></param>
    private void DrawAllLinesLine(VertexHelper vh)
    {
        var uv0 = GetUnderlineCharUV();
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var endIndex = 0;
            if (i + 1 < lines.Length)
            {
                //通过下一行的起始索引减1得到这一行最后一个字符索引位置
                var nextLineStartCharIdx = lines[i + 1].startCharIdx;
                //与本行的相同,当文本宽度只够容纳一个字的时候，unity会产生一个空行，要排除改行
                if (nextLineStartCharIdx == lines[i].startCharIdx) continue;
                endIndex = nextLineStartCharIdx - 1;
            }
            else
            {
                //最后一行的最后字符索引位置
                //需要判断当前行是否是全部文本里的最后一行
                var totalCount = text.text.Length;
                endIndex = characterCountVisible;
                if (characterCountVisible == totalCount)
                {
                    endIndex--;
                }
            }

            var bottomY = GetLineBottomY(i);
            var firstCharOff = 0;
            AddUnderlineVertTriangle(vh, line.startCharIdx, endIndex, firstCharOff, bottomY, uv0);
        }
    }

    private float GetLineBottomY(int lineIndex)
    {
        UILineInfo line = this.lines[lineIndex];
        var bottomY = line.topY - (Strickout ? line.height * .5f : line.height);

        //bottomY是原始大小下的信息,但文本在不同分辨率下会被进一步缩放处理,所以要将比例带入计算
        bottomY /= this.text.pixelsPerUnit;
        return bottomY;
    }

    private Vector2 GetCharCursorPos(int charIdx, float firstCharOff)
    {
        var charInfo = this.characters[charIdx];
        var cursorPos = charInfo.cursorPos;
        //cursorPos是原始大小下的信息,但文本在不同分辨率下会被进一步缩放处理,所以要将比例带入计算
        cursorPos /= this.text.pixelsPerUnit;

        var rtf = (this.transform as RectTransform);
        return cursorPos;
    }

    private UIVertex[] underlineUIVertexs = new UIVertex[6];

    private void AddUnderlineVertTriangle(VertexHelper vh, int startIndex, int endIndex, float firstCharOff,
        float bottomY, Vector2 uv0)
    {
        if (textChars[endIndex] == '\n')
        {
            //跳过换行符
            endIndex--;
        }

        if (endIndex < startIndex) return;
        //左上
        var pos0 = new Vector3(GetCharCursorPos(startIndex, firstCharOff).x, bottomY, 0);
        //左下, 向下扩展
        var pos1 = new Vector3(pos0.x, pos0.y - this.lineHeight, 0);
        //右下. charWidth是原始大小下的信息,但文本在不同分辨率下会被进一步缩放处理,所以要将比例带入计算
        var pos2 = new Vector3(
            GetCharCursorPos(endIndex, firstCharOff).x + characters[endIndex].charWidth / text.pixelsPerUnit,
            pos1.y,
            0);
        //右上
        var pos3 = new Vector3(pos2.x, pos0.y, 0);

        //按照stream存储的规范,构建6个顶点: 左上和右下是2个三角形的重叠, 
        UIVertex vert = UIVertex.simpleVert;
        vert.color = this.lineColor;
        vert.uv0 = uv0;

        vert.position = pos0;
        underlineUIVertexs[0] = vert;
        underlineUIVertexs[3] = vert;

        vert.position = pos1;
        underlineUIVertexs[5] = vert;

        vert.position = pos2;
        underlineUIVertexs[2] = vert;
        underlineUIVertexs[4] = vert;

        vert.position = pos3;
        underlineUIVertexs[1] = vert;

        stream.AddRange(underlineUIVertexs);
    }


    //------------------------------------------暂时不用的功能

    //[Header("是否使用customLineIndexArray来作为显示线的起止依据,否则是全文字段显示")]
    // private bool useCustomLineIndexArray = false;
    private Vector2Int[] customLineIndexArray = new Vector2Int[] { };

    /// <summary>
    /// 自定义下划线
    /// </summary>
    private void DrawCustomLine(VertexHelper vh)
    {
        var uv0 = this.GetUnderlineCharUV();
        var charsMaxIndex = this.characterCountVisible - 1;

        var chars = text.text.ToCharArray();

        for (int i = 0; i < this.customLineIndexArray.Length; i++)
        {
            var v2 = this.customLineIndexArray[i];
            var startIndex = v2[0];
            var endIndex = v2[1];

            if (endIndex < startIndex)
            {
                startIndex = v2[1];
                endIndex = v2[0];
            }

            if (startIndex < 0) startIndex = 0;
            if (endIndex > charsMaxIndex) endIndex = charsMaxIndex;

            if (startIndex >= this.characterCountVisible) continue;

            var lineIndex0 = this.GetCharInLineIndex(startIndex);
            var lineIndex1 = this.GetCharInLineIndex(endIndex);
            if (lineIndex0 != lineIndex1)
            {
                // 跨行
                for (int j = lineIndex0; j <= lineIndex1; j++)
                {
                    var bottomY = this.GetLineBottomY(j);
                    var lineStartCharIndex = startIndex;
                    var lineEndCharIndex = endIndex;
                    if (j == lineIndex0)
                    {
                        // 第一行,从指定起始字索引到改行末尾字索引(改行末尾索引是下一行的起始索引-1得到)
                        lineEndCharIndex = lines[j + 1].startCharIdx - 1;
                    }
                    else if (j == lineIndex1)
                    {
                        // 最后一行,从改行起始字索引到指定字索引
                        lineStartCharIndex = lines[j].startCharIdx;
                    }
                    else
                    {
                        // 中间行,从改行起始字所以到该行末尾字索引
                        lineStartCharIndex = lines[j].startCharIdx;
                        lineEndCharIndex = lines[j + 1].startCharIdx - 1;
                    }

                    var firstCharOff = 0;
                    this.AddUnderlineVertTriangle(vh, lineStartCharIndex, lineEndCharIndex, firstCharOff, bottomY,
                        uv0);
                }
            }
            else
            {
                // 在同一行
                var bottomY = this.GetLineBottomY(lineIndex0);
                var firstCharOff = 0;
                this.AddUnderlineVertTriangle(vh, startIndex, endIndex, firstCharOff, bottomY, uv0);
            }
        }
    }
}
