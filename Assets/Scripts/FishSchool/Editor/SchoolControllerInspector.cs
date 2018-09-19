using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FishSchool
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FishSchoolController))]
    public class SchoolControllerInspector : Editor
    {
        FishSchoolController fsc;
        SerializedProperty children;
        SerializedProperty newGameObject;
        SerializedProperty bubbles;
        SerializedProperty avoidanceMask;
        SerializedProperty fleeMaterial;
        SerializedProperty fleeAcceleration;
        SerializedProperty flee;

        private Color sectionDone = new Color(0.65f, 0.65f, 0.65f);
        private Color sectionUnfinished = new Color(0.65f, 0.35f, 0.35f);

        private void OnEnable()
        {
            fsc = (FishSchoolController)target;
            children = serializedObject.FindProperty("children");
            newGameObject = serializedObject.FindProperty("newGameObject");
            bubbles = serializedObject.FindProperty("_bubbles");
            avoidanceMask = serializedObject.FindProperty("avoidanceMask");
            fleeMaterial = serializedObject.FindProperty("fleeMaterial");
            fleeAcceleration = serializedObject.FindProperty("fleeAcceleration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();


            SectionPrefabs();
            SectionGrouping();
            SectionAreaSize();
            SectionSpeedAndMovement();
            SectionWaypoints();
            SectionFlee();
            SectionAvoidance();
            SectionPush();

            if (EditorGUI.EndChangeCheck())
                Undo.RegisterCompleteObjectUndo(fsc, "Player name change");

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) EditorUtility.SetDirty(fsc);
        }

        private void SectionPrefabs()
        {
            Header("Prefabs", sectionDone);

            serializedObject.ApplyModifiedProperties();
            fsc.childAmount = EditorGUILayout.IntSlider("Fish Amount", fsc.childAmount, 1, 1000);
            fsc.fishSize = EditorGUILayout.Vector2Field("Random size", fsc.fishSize);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(children, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void SectionGrouping()
        {
            Header("Grouping", sectionDone);
            var text = new string[] { "None", "To School", "Existing object", "New object" };
            fsc.grouping = (FishSchoolController.groupingType)GUILayout.SelectionGrid((int)fsc.grouping, text, 4, EditorStyles.radioButton, GUILayout.MinWidth(1));
            GUILayout.Space(2);
            if (fsc.grouping == FishSchoolController.groupingType.Existing)
                EditorGUILayout.PropertyField(newGameObject, new GUIContent("Object to parent"));
            else if (fsc.grouping == FishSchoolController.groupingType.NewObject)
                fsc.groupName = EditorGUILayout.TextField("Group Name", fsc.groupName);

            EditorGUILayout.EndVertical();
        }

        private void SectionAreaSize()
        {
            Header("Area", sectionDone);
            EditorGUIUtility.labelWidth = 170;
            fsc.spawnArea = EditorGUILayout.Vector3Field("School Size (spawn area)", fsc.spawnArea);
            fsc.roamingSize = EditorGUILayout.Vector3Field("Roaming area", fsc.roamingSize);
            fsc.initialPosition = EditorGUILayout.Vector3Field("Start position offset", fsc.initialPosition);
            fsc.forceInitialWaypoint = EditorGUILayout.Toggle("Force initial waypoint", fsc.forceInitialWaypoint);
            EditorGUILayout.EndVertical();
        }

        private void SectionSpeedAndMovement()
        {
            Header("Speed & Movement", sectionDone);
            EditorGUIUtility.labelWidth = 200;
            fsc.fishSchoolSpeedFactor = EditorGUILayout.Vector2Field("Speed Fish School Multiplier", fsc.fishSchoolSpeedFactor);
            fsc.childSpeedRandom = EditorGUILayout.Vector2Field("Speed Child Multiplier", fsc.childSpeedRandom);
            fsc.acceleration = EditorGUILayout.Slider("Fish Acceleration", fsc.acceleration, 0.001f, 0.07f);
            fsc.brake = EditorGUILayout.Slider("Fish Brake Power", fsc.brake, 0.001f, 0.025f);
            fsc.dampingSpeed = EditorGUILayout.Vector2Field("Fish Turn Speed", fsc.dampingSpeed);
            EditorGUILayout.EndVertical();
        }

        private void SectionWaypoints()
        {
            Header("Waypoints", sectionDone);
            EditorGUIUtility.labelWidth = 280;

            fsc.fishTriggersNewWaypoint = EditorGUILayout.Toggle("Fish Triggers Waypoint", fsc.fishTriggersNewWaypoint);

            if (fsc.fishTriggersNewWaypoint)
            {
                EditorGUI.indentLevel = 2;
                fsc.minWaypointDistance = EditorGUILayout.FloatField("Distance To Waypoint", fsc.minWaypointDistance);
                EditorGUI.indentLevel = 0;
            }

            fsc.autoWaypointBytime = EditorGUILayout.Toggle("Create School Waypoint Randomly by time", fsc.autoWaypointBytime);
            GetVector2Field(fsc.autoWaypointBytime, ref fsc.randomWaypointTime);
            fsc.changeChildWaypoints = EditorGUILayout.Toggle("Force Fish Waypoints", fsc.changeChildWaypoints);
            GetVector2Field(fsc.changeChildWaypoints, ref fsc.randomChildDelay);
            EditorGUILayout.EndVertical();
        }

        private void GetVector2Field(bool state, ref Vector2 vector2Field)
        {
            if (state)
            {
                EditorGUI.indentLevel = 2;
                vector2Field = EditorGUILayout.Vector2Field("Distance To Waypoint", vector2Field);
                EditorGUI.indentLevel = 0;
            }
        }

        private void SectionFlee()
        {
            Header("Fleeing", sectionDone);
            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.PropertyField(fleeMaterial, new GUIContent("Flee material"));
            fsc.speedFleeMultiplier = EditorGUILayout.Vector2Field("Flee Speed", fsc.speedFleeMultiplier);
            fsc.speedTurn = EditorGUILayout.FloatField("Flee Speed", fsc.speedTurn);
            EditorGUILayout.PropertyField(fleeAcceleration, new GUIContent("Flee Acceleration"));
            fsc.timeToRelax = EditorGUILayout.Vector2Field("Time to stop fleeing", fsc.timeToRelax);
            EditorGUILayout.EndVertical();
        }

        private void SectionAvoidance()
        {
            Header("Avoidance", sectionDone);
            fsc.isAvoidance = EditorGUILayout.Toggle("Avoidance enabled", fsc.isAvoidance);
            if (fsc.isAvoidance)
            {
                EditorGUILayout.PropertyField(avoidanceMask, new GUIContent("Collider Mask"));
                fsc.avoidAngle = EditorGUILayout.Slider("Avoid Angle", fsc.avoidAngle, 0.05f, 0.95f);
                fsc.avoidDistance = EditorGUILayout.FloatField("Avoid Distance", fsc.avoidDistance);
                if (fsc.avoidDistance <= 0.1) fsc.avoidDistance = 0.1f;
                fsc.avoidSpeed = EditorGUILayout.FloatField("Avoid Speed", fsc.avoidSpeed);
                fsc.stopDistance = EditorGUILayout.FloatField("Stop Distance", fsc.stopDistance);
                fsc.stopSpeedMultiplier = EditorGUILayout.FloatField("Stop Speed Multiplier", fsc.stopSpeedMultiplier);
                if (fsc.stopDistance <= 0.1) fsc.stopDistance = 0.1f;
            }

            EditorGUILayout.EndVertical();
        }

        private void SectionPush()
        {
            Header("Push", sectionUnfinished);
            fsc.isPush = EditorGUILayout.Toggle("Push enabled", fsc.isPush);
            if (fsc.isPush)
            {
                fsc._pushDistance = EditorGUILayout.FloatField("Push Distance", fsc._pushDistance);
                if (fsc._pushDistance <= 0.1) fsc._pushDistance = 0.1f;
                fsc._pushForce = EditorGUILayout.FloatField("Push Force", fsc._pushForce);
                if (fsc._pushForce <= 0.01) fsc._pushForce = 0.01f;
            }
            EditorGUILayout.EndVertical();
        }

        private GUIStyle GetGUIStyle(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = color;
            style.fontStyle = FontStyle.Bold;

            return style;
        }

        private void Header(string title, Color color)
        {
            GUI.color = color;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUI.color = Color.white;
            //if (EditorApplication.isPlaying) GUI.enabled = false;
        }
    }
}