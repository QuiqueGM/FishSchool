using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FishSchool
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FSProperties))]
    public class FSPropertiesInspector : Editor
    {
        FSProperties fsc;
        SerializedProperty children;
        SerializedProperty newGameObject;
        SerializedProperty bubbles;
        SerializedProperty avoidanceMask;
        SerializedProperty turnAcceleration;
        SerializedProperty fleeMaterial;
        SerializedProperty fleeAcceleration;
        SerializedProperty flee;

        private Color sectionDone = new Color(0.65f, 0.65f, 0.65f);
        private Color sectionUnfinished = new Color(0.65f, 0.35f, 0.35f);

        private void OnEnable()
        {
            fsc = (FSProperties)target;
            children = serializedObject.FindProperty("children");
            newGameObject = serializedObject.FindProperty("newGameObject");
            bubbles = serializedObject.FindProperty("_bubbles");
            avoidanceMask = serializedObject.FindProperty("avoidanceMask");
            turnAcceleration = serializedObject.FindProperty("turnAcceleration");
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
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(children, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void SectionGrouping()
        {
            Header("Grouping", sectionDone);
            var text = new string[] { "None", "To School", "Existing object", "New object" };
            fsc.grouping = (FSProperties.GroupingType)GUILayout.SelectionGrid((int)fsc.grouping, text, 4, EditorStyles.radioButton, GUILayout.MinWidth(1));
            GUILayout.Space(2);
            if (fsc.grouping == FSProperties.GroupingType.Existing)
                EditorGUILayout.PropertyField(newGameObject, new GUIContent("Object to parent"));
            else if (fsc.grouping == FSProperties.GroupingType.NewObject)
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
            if (fsc.forceInitialWaypoint) InitialPoint();
            EditorGUILayout.EndVertical();
        }

        private void InitialPoint()
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(30);
                var text = new string[] { "Minimum distance", "Specific", "Randomly" };
                fsc.initialWaypoint = (FSProperties.InitialWaypointType)GUILayout.SelectionGrid((int)fsc.initialWaypoint, text, 3, EditorStyles.radioButton);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = 2;
            if (fsc.initialWaypoint == FSProperties.InitialWaypointType.ByMinDistance)
            {
                EditorGUIUtility.labelWidth = 300;
                fsc.minDistanceToInitialWaypoint = EditorGUILayout.FloatField("Minimum distance to start position offset", fsc.minDistanceToInitialWaypoint);
            }
            else if (fsc.initialWaypoint == FSProperties.InitialWaypointType.Specific)
            {
                EditorGUIUtility.labelWidth = 200;
                fsc.specificInitialPoint = EditorGUILayout.Vector3Field("Specific point", fsc.specificInitialPoint);
            }
            EditorGUI.indentLevel = 0;
        }

        private void SectionSpeedAndMovement()
        {
            Header("Size & Movement", sectionDone);
            EditorGUIUtility.labelWidth = 200;
            fsc.fishSize = EditorGUILayout.Vector2Field("Fish size", fsc.fishSize);
            fsc.fishSchoolSpeed = EditorGUILayout.Vector2Field("Speed Fish School Multiplier", fsc.fishSchoolSpeed);
            fsc.childSpeed = EditorGUILayout.Vector2Field("Speed Child Multiplier", fsc.childSpeed);
            //fsc.acceleration = EditorGUILayout.Slider("Fish Acceleration", fsc.acceleration, 0.001f, 0.07f);
            //fsc.brake = EditorGUILayout.Slider("Fish Brake Power", fsc.brake, 0.001f, 0.025f);
            fsc.turnSpeed = EditorGUILayout.Vector2Field("Fish Turn Speed", fsc.turnSpeed);
            EditorGUILayout.PropertyField(turnAcceleration, new GUIContent("Flee Acceleration"));
            EditorGUILayout.EndVertical();
        }

        private void SectionWaypoints()
        {
            Header("Waypoints", sectionDone);
            EditorGUIUtility.labelWidth = 280;

            fsc.fishTriggersNewWaypoint = EditorGUILayout.Toggle("Fish Triggers Waypoint", fsc.fishTriggersNewWaypoint);
            GetFloatField(fsc.fishTriggersNewWaypoint, "Distance to waypoint", ref fsc.minWaypointDistance, true, "(Activating this option might decrease the performance)");
            fsc.autoWaypointBytime = EditorGUILayout.Toggle("Create School Waypoint Randomly by time", fsc.autoWaypointBytime);
            GetVector2Field(fsc.autoWaypointBytime, "Time in seconds", ref fsc.randomWaypointTime);
            fsc.changeChildWaypoints = EditorGUILayout.Toggle("Force Fish Waypoints", fsc.changeChildWaypoints);
            GetFloatField(fsc.changeChildWaypoints, "Threshold", ref fsc.threshold, false, string.Empty);
            EditorGUILayout.EndVertical();
        }

        private void GetFloatField(bool state, string label, ref float floatField, bool showLabel, string message)
        {
            if (state)
            {
                EditorGUI.indentLevel = 2;
                floatField = EditorGUILayout.FloatField(label, floatField);
                if (showLabel)
                    EditorGUILayout.LabelField(message, EditorStyles.miniLabel);
                EditorGUI.indentLevel = 0;
                GUILayout.Space(3);
            }
        }

        private void GetVector2Field(bool state, string label, ref Vector2 vector2Field)
        {
            if (state)
            {
                EditorGUI.indentLevel = 2;
                vector2Field = EditorGUILayout.Vector2Field(label, vector2Field);
                EditorGUI.indentLevel = 0;
                GUILayout.Space(3);
            }
        }

        private void SectionFlee()
        {
            Header("Flee", sectionDone);
            EditorGUIUtility.labelWidth = 150;
            fsc.isFleeEnabled = EditorGUILayout.Toggle("Flee enabled", fsc.isFleeEnabled);
            if (fsc.isFleeEnabled)
            {
                EditorGUILayout.PropertyField(fleeMaterial, new GUIContent("Flee material"));
                fsc.speedFleeMultiplier = EditorGUILayout.Vector2Field("Flee Speed", fsc.speedFleeMultiplier);
                fsc.speedTurn = EditorGUILayout.Vector2Field("Turn Speed", fsc.speedTurn);
                EditorGUILayout.PropertyField(fleeAcceleration, new GUIContent("Flee Acceleration"));
                fsc.timeToRelax = EditorGUILayout.Vector2Field("Time to stop fleeing", fsc.timeToRelax);
            }
            EditorGUILayout.EndVertical();
        }

        private void SectionAvoidance()
        {
            Header("Avoidance", sectionDone);
            fsc.isAvoidance = EditorGUILayout.Toggle("Avoidance enabled", fsc.isAvoidance);
            if (fsc.isAvoidance)
            {
                EditorGUILayout.PropertyField(avoidanceMask, new GUIContent("Collider Mask"));
                fsc.avoidanceDetection = EditorGUILayout.FloatField("Detection distance", fsc.avoidanceDetection);
                fsc.avoidanceStopDistance = EditorGUILayout.FloatField("Stop Distance", fsc.avoidanceStopDistance);
                CheckAvoidingValues();
                //fsc.avoidAngle = EditorGUILayout.Slider("Avoid Angle", fsc.avoidAngle, 0, 1);
                //if (fsc.avoidDistance <= 0.1) fsc.avoidDistance = 0.1f;
                //fsc.avoidSpeed = EditorGUILayout.FloatField("Avoid Speed", fsc.avoidSpeed);
                //fsc.stopDistance = EditorGUILayout.FloatField("Stop Distance", fsc.stopDistance);
                //fsc.stopSpeedMultiplier = EditorGUILayout.FloatField("Stop Speed Multiplier", fsc.stopSpeedMultiplier);
                //if (fsc.stopDistance <= 0.1) fsc.stopDistance = 0.1f;
            }
            EditorGUILayout.EndVertical();
        }

        private void CheckAvoidingValues()
        {
            if (fsc.avoidanceDetection < fsc.avoidanceStopDistance)
                fsc.avoidanceDetection = fsc.avoidanceStopDistance;
            float scale = (fsc.fishSize.x + fsc.fishSize.y) / 2;
            if (fsc.avoidanceStopDistance < scale)
                EditorGUILayout.LabelField("This distance will cause objects penetrating into others", EditorStyles.miniLabel);
            if (fsc.avoidanceStopDistance < 0) fsc.avoidanceStopDistance = 0.05f;
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