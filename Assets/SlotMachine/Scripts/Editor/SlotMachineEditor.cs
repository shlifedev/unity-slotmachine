using DG.Tweening;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif 

#if UNITY_EDITOR
[CustomEditor(typeof(SlotMachine))]
public class SlotMachineEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        var t = (target) as SlotMachine;
        //AnimationCurve curve = new AnimationCurve(); 
        DrawDefaultInspector();
        float timeSum = 0;
        float cntSum = 0;
        for (int i = 0; i < t.smTimes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"페이즈{i} 속도");
            var speed = t.smTimes[i].time / t.smTimes[i].count;
            timeSum += t.smTimes[i].time;
            cntSum += t.smTimes[i].count;

            EditorGUILayout.FloatField(speed);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"총 재생시간" + timeSum.ToString("0.00"));
        EditorGUILayout.LabelField($"총 재생횟수: " + cntSum);
        EditorGUILayout.EndHorizontal();
        //EditorGUILayout.CurveField(curve);
    }
}
#endif