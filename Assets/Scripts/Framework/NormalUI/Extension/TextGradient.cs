using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Text))]
public class TextGradient : BaseMeshEffect
{
    public enum GradientDirection
    {
        Horizontal,
        Vertical,
        LeftUpToRightDown,
        LeftDownToRightUp
    }

    [SerializeField] //�������л����ԣ�һ���Editor����Ҫͨ���������������ҵ����ֶ�
    public Color32 topColor = Color.white; //������ɫ

    [SerializeField] public Color32 bottomColor = Color.black; //�ײ���ɫ
    [SerializeField] public GradientDirection gradientDirection; //���䷽��
    [SerializeField] public bool useEffect = false; //�Ƿ�ʹ����Ӱ
    [SerializeField] public Color effectColor = new Color(0f, 0f, 0f, 0.5f); //��Ӱ��ɫ
    [SerializeField] public Vector2 effectDistance = new Vector2(1f, -1f); //��Ӱƫ��

    private const int DefautlVertexNumPerFont = 6; //������

    List<UIVertex> vertexBuffers = new List<UIVertex>();
    public Mesh mesh = null;


    /// <summary>
    /// ��������ɫ
    /// </summary>
    /// <param name="vertexList"></param>
    /// <param name="index"></param>
    /// <param name="color"></param>
    private void ModifyVertexColor(List<UIVertex> vertexList, int index, Color color)
    {
        UIVertex temp = vertexList[index];
        temp.color = color;
        vertexList[index] = temp;
    }

    //�޸�����ʱ����
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        vh.GetUIVertexStream(vertexBuffers); //��ȡ����

        //��Inspector����ʾ����
        if (mesh == null)
        {
            mesh = new Mesh();
        }

        vh.FillMesh(mesh);


        int count = vertexBuffers.Count;
        if (count > 0)
        {
            /**��������ɫ( ������Ҫ���׶����˳�� )
            *   5-0 ---- 1
            *    | \    |
            *    |  \   |
            *    |   \  |
            *    |    \ |
            *    4-----3-2
            **/
            for (int i = 0; i < count; i += DefautlVertexNumPerFont)
            {
                //�ֱ�����ÿ���������ɫ
                switch (gradientDirection)
                {
                    case GradientDirection.Horizontal:
                        ModifyVertexColor(vertexBuffers, i, topColor);
                        ModifyVertexColor(vertexBuffers, i + 1, topColor);
                        ModifyVertexColor(vertexBuffers, i + 2, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 3, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 4, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 5, topColor);
                        break;
                    case GradientDirection.Vertical:
                        ModifyVertexColor(vertexBuffers, i, topColor);
                        ModifyVertexColor(vertexBuffers, i + 1, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 2, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 3, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 4, topColor);
                        ModifyVertexColor(vertexBuffers, i + 5, topColor);
                        break;
                    case GradientDirection.LeftUpToRightDown:
                        ModifyVertexColor(vertexBuffers, i, topColor);
                        ModifyVertexColor(vertexBuffers, i + 1, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 2, topColor);
                        ModifyVertexColor(vertexBuffers, i + 3, topColor);
                        ModifyVertexColor(vertexBuffers, i + 4, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 5, topColor);
                        break;
                    case GradientDirection.LeftDownToRightUp:
                        ModifyVertexColor(vertexBuffers, i, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 1, topColor);
                        ModifyVertexColor(vertexBuffers, i + 2, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 3, bottomColor);
                        ModifyVertexColor(vertexBuffers, i + 4, topColor);
                        ModifyVertexColor(vertexBuffers, i + 5, bottomColor);
                        break;
                    default:
                        break;
                }
            }
        }

        if (useEffect) //�Ƿ�ʹ����Ӱ���������Ҫ��Ӱ���ܿ����ⲿ�ִ���ɾ����
        {
            //����һ���Ķ�������
            var neededCapacity = vertexBuffers.Count * 2;
            if (vertexBuffers.Capacity < neededCapacity)
                vertexBuffers.Capacity = neededCapacity;

            for (int i = 0, cnt = vertexBuffers.Count; i < cnt; ++i)
            {
                var vt = vertexBuffers[i];
                vertexBuffers.Add(vt);

                Vector3 v = vt.position;
                v.x += effectDistance.x;
                v.y += effectDistance.y;
                vt.position = v;
                vt.color = effectColor;
                vertexBuffers[i] = vt;
            }
        }

        vh.Clear();
        //���������VertexHelper���������������ζ�������,�����ĳ��ȱ��������ı���
        vh.AddUIVertexTriangleStream(vertexBuffers);
    }

    /// <summary>
    /// ��Scene����ʾ����
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red; //������ɫ
        for (int i = 0; i < vertexBuffers.Count; i++)
        {
            //��mesh����תΪ��������
            Vector3 targetPosition = transform.TransformPoint(vertexBuffers[i].position);
            Gizmos.DrawSphere(targetPosition, 5f);
        }
    }
}
