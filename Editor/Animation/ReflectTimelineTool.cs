using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Reflect;
using System.Linq;
using KeyframeUtility;

namespace UnityEditor.Reflect.Timeliner
{
    public class ReflectTimelineTool : EditorWindow
    {
        string startKeyString = "Contained in Task Start (Planned):1";
        string endKeyString = "Contained in Task End (Planned):1";

        float timeScale = 1f;

        int daysBefore = 3;
        int daysAfter = 3;

        [UnityEditor.MenuItem("Reflect/Timeliner Tool")]
        static void Init()
        {
            ReflectTimelineTool window = ReflectTimelineTool.GetWindow<ReflectTimelineTool>();
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            startKeyString = EditorGUILayout.TextField("Start Key", startKeyString);
            endKeyString = EditorGUILayout.TextField("End Key", endKeyString);
            timeScale = EditorGUILayout.FloatField("Time Scale (days/second)", timeScale);
            daysBefore = EditorGUILayout.IntField("Pre-roll (days)", daysBefore);
            daysAfter = EditorGUILayout.IntField("Post-roll (days)", daysAfter);

            if (GUILayout.Button("Create Animation"))
            {
                if (Selection.activeGameObject == null)
                {
                    Debug.LogWarning("No GameObject Selected.");
                    return;
                }

                //PlayableDirector director = Selection.activeGameObject.GetComponent<PlayableDirector>() ?? Selection.activeGameObject.AddComponent<PlayableDirector>();

                // get or create the Director
                var director = Selection.activeGameObject.GetComponent<PlayableDirector>();
                if (!director)
                    director = Selection.activeGameObject.AddComponent<PlayableDirector>();

                // get or create the Animator
                var animator = Selection.activeGameObject.GetComponent<Animator>();
                if (!animator)
                    animator = Selection.activeGameObject.AddComponent<Animator>();

                // get or create the TimelineAsset
                TimelineAsset playableAsset = (TimelineAsset)director.playableAsset;
                if (!playableAsset)
                {
                    //playableAsset = new TimelineAsset();
                    playableAsset = TimelineAsset.CreateInstance<TimelineAsset>();
                    var path = Path.Combine(Application.dataPath, "Playables");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(playableAsset, string.Format("Assets/Playables/{0}_Timeline.asset", director.gameObject.name));
                    AssetDatabase.SaveAssets();
                    director.playableAsset = playableAsset;
                }

                // get or create the AnimationTrack
                AnimationTrack track = playableAsset.rootTrackCount > 0 ? (AnimationTrack)playableAsset.GetRootTrack(0) : null;
                if (!track)
                    track = playableAsset.CreateTrack<AnimationTrack>("Timeliner");

                director.SetGenericBinding(track, animator);

                AnimationClip animationClip;
                if (!track.hasClips)
                {
                    animationClip = new AnimationClip();
                    animationClip.wrapMode = WrapMode.Clamp;

                    var path = Path.Combine(Application.dataPath, "Playables");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(animationClip, string.Format("Assets/Playables/{0}_AnimationClip.asset", director.gameObject.name));
                    AssetDatabase.SaveAssets();

                    track.CreateClip(animationClip);
                }
                else
                {
                    animationClip = (track.GetClips().First().asset as AnimationPlayableAsset).clip;
                }

                var metadatas = Selection.activeGameObject.GetComponentsInChildren<Metadata>();

                List<DateTime> dates = new List<DateTime>();

                for (int i = 0; i < metadatas.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Collecting Dates", metadatas[i].gameObject.name, i / metadatas.Length);
                    var p = metadatas[i].GetParameters();
                    if (p.TryGetValue(startKeyString, out Metadata.Parameter startDate))
                    {
                        dates.Add(DateTime.Parse(startDate.value));
                    }
                }

                var earliestDate = dates.Min().AddDays(-daysBefore);

                for (int i = 0; i < metadatas.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Adding Keys to Animation Clip", metadatas[i].gameObject.name, i / metadatas.Length);
                    var p = metadatas[i].GetParameters();
                    if (p.TryGetValue(startKeyString, out Metadata.Parameter startDate))
                    {
                        AnimationCurve constantCurve = new AnimationCurve();
                        constantCurve.AddKey(KeyframeUtil.GetNew(0f, 0f, TangentMode.Stepped));
                        constantCurve.AddKey(KeyframeUtil.GetNew((float)(DateTime.Parse(startDate.value) - earliestDate).TotalDays * (1f / timeScale), 1f, TangentMode.Stepped));

                        // TODO : get proper object relative path in case hierarchy gets deeper than one level
                        animationClip.SetCurve(metadatas[i].gameObject.name, typeof(GameObject), "m_IsActive", constantCurve);
                    }
                }

                EditorUtility.ClearProgressBar();

                track.GetClips().Min().duration = animationClip.length;
            }
        }
    }
}