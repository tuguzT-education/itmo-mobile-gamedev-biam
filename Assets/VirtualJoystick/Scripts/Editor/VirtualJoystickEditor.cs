using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Terresquall {
    [CustomEditor(typeof(VirtualJoystick))]
    [CanEditMultipleObjects]
    public class VirtualJoystickEditor : Editor {

        VirtualJoystick joystick;
        RectTransform rectTransform;
        Canvas canvas;

        private int scaleFactor;
        private static readonly List<int> usedIDs = new List<int>();

        void OnEnable() {
            joystick = target as VirtualJoystick;
            rectTransform = joystick.GetComponent<RectTransform>();
            canvas = joystick.GetComponentInParent<Canvas>();
        }

        // Does the passed joystick have an ID that is unique to itself?
        bool HasUniqueID(VirtualJoystick vj) {
            foreach(VirtualJoystick v in FindObjectsOfType<VirtualJoystick>()) {
                if(v == vj) continue;
                if(v.ID == vj.ID) return false;
            }
            return true;
        }

        // Is a given ID value already used by another joystick?
        bool IsAvailableID(int id) {
            foreach(VirtualJoystick v in FindObjectsOfType<VirtualJoystick>()) {
                if(v.ID == id) return false;
            }
            return true;
        }

        // Do all the joysticks have unique IDs.
        bool HasRepeatIDs() {
            usedIDs.Clear();
            foreach(VirtualJoystick vj in FindObjectsOfType<VirtualJoystick>()) {
                if(usedIDs.Contains(vj.ID)) return true;
                usedIDs.Add(vj.ID);
            }
            return false;
        }

        // Reassign all IDs for all Joysticks.
        void ReassignAllIDs(VirtualJoystick exception = null) {
            foreach(VirtualJoystick vj in FindObjectsOfType<VirtualJoystick>()) {
                // Ignore joysticks that are already unique.
                if(exception == vj || HasUniqueID(vj)) continue;
                ReassignThisID(vj);
            }
        }

        // Reassign the ID for this Joystick only.
        void ReassignThisID(VirtualJoystick vj) {

            // Save the action in the History.
            Undo.RecordObject(vj, "Generate Unique Joystick ID");

            // Get all joysticks so that we can check against it if the ID is valid.
            VirtualJoystick[] joysticks = FindObjectsOfType<VirtualJoystick>();
            for(int i = 0; i < joysticks.Length; i++) {
                if(IsAvailableID(i)) {
                    vj.ID = i; // If we find an unused ID, use it.
                    EditorUtility.SetDirty(vj);
                    return;
                }
            }

            // If all of the IDs are used, we will have to use length + 1 as the ID.
            vj.ID = joysticks.Length;
            EditorUtility.SetDirty(vj);
        }

        override public void OnInspectorGUI() {

            // Show an error text box if the new Input System is being used.
            try {
                Vector2 v = Input.mousePosition;
            } catch(System.InvalidOperationException e) {

                Texture2D icon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;

                // Create a horizontal layout for the icon and text
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    // Display the icon
                    GUILayout.Label(icon, GUILayout.Width(40), GUILayout.Height(40));

                    // Display the text
                    GUIStyle ts = new GUIStyle(EditorStyles.label);
                    ts.wordWrap = true;
                    ts.richText = true;
                    ts.alignment = TextAnchor.MiddleLeft;
                    ts.fontSize = EditorStyles.helpBox.fontSize;
                    GUILayout.Label("<b>This component does not work with the new Input System.</b> You will need to re-enable the old Input System by going to <b>Project Settings > Player > Other Settings > Active Input Handling</b> and selecting <b>Both</b>.", ts);
                    Debug.LogWarning(e.Message, this);
                }
                GUILayout.EndHorizontal();
            }

            // Draw a help text box if this is not attached to a Canvas.
            if(!canvas && !EditorUtility.IsPersistent(target)) {
                EditorGUILayout.HelpBox("This GameObject needs to be parented to a Canvas, or it won't work!", MessageType.Warning);
            }

            // Draw all the inspector properties.
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            bool snapsToTouch = true;
            int directions = 0;
            if (property.NextVisible(true)) {
                do {
                    // If the property name is snapsToTouch, record its value.
                    switch(property.name) {
                        case "m_Script":
                            continue;
                        case "snapsToTouch":
                            snapsToTouch = property.boolValue;
                            break;
                        case "directions":
                            directions = property.intValue;
                            break;
                        case "boundaries":
                            // If snapsToTouch is off, don't render boundaries.
                            if(!snapsToTouch) continue;
                            break;
                        case "angleOffset":
                            if(directions <= 0) continue;
                            break;
                    }

                    
                    EditorGUI.BeginChangeCheck();

                    // Print different properties based on what the property is.
                    if(property.name == "angleOffset") {
                        float maxAngleOffset = 360f / directions / 2;
                        EditorGUILayout.Slider(property, -maxAngleOffset, maxAngleOffset, new GUIContent("Angle Offset"));
                    } else {
                        EditorGUILayout.PropertyField(property, true);
                    }

                    EditorGUI.EndChangeCheck();

                    // If the property is an ID, show a button allowing us to reassign the IDs.
                    if(property.name == "ID" && !EditorUtility.IsPersistent(target)) {
                        if(!HasUniqueID(joystick)) {
                            EditorGUILayout.HelpBox("This Virtual Joystick doesn't have a unique ID. Please assign a unique ID or click on the button below.", MessageType.Warning);
                            if(GUILayout.Button("Generate Unique Joystick ID")) {
                                ReassignThisID(joystick);
                            }
                            EditorGUILayout.Space();
                        } else if(HasRepeatIDs()) {
                            EditorGUILayout.HelpBox("At least one of your Virtual Joysticks doesn't have a unique ID. Please ensure that all of them have unique IDs, or they may not be able to collect input properly.", MessageType.Warning);
                            EditorGUILayout.Space();
                        }
                    }
                    
                } while (property.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();

            //Increase Decrease buttons
            if(joystick) {

                if(!joystick.controlStick) {
                    EditorGUILayout.HelpBox("There is no Control Stick assigned. This joystick won't work.", MessageType.Warning);
                    return;
                }

                // Add the heading for the size adjustments.
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Size Adjustments");
                GUILayout.BeginHorizontal();

                // Create the Increase / Decrease Size buttons and code the actions.
                bool increaseSize = GUILayout.Button("Increase Size", EditorStyles.miniButtonLeft),
                     decreaseSize = GUILayout.Button("Decrease Size", EditorStyles.miniButtonRight);

                if(increaseSize || decreaseSize) {
                    // Calculate the sizes needed for the increment / decrement.
                    int gcd = Mathf.RoundToInt(FindGCD((int)rectTransform.sizeDelta.x, (int)joystick.controlStick.rectTransform.sizeDelta.x));
                    Vector2 denominator = new Vector2(gcd, gcd);

                    // Record actions for all elements.
                    RectTransform[] affected = rectTransform.GetComponentsInChildren<RectTransform>();
                    RecordSizeChangeUndo(affected);

                    // Increase / decrease size actions.
                    if(increaseSize) {
                        foreach(RectTransform r in affected)
                            r.sizeDelta += r.sizeDelta / denominator;
                    } else if(decreaseSize) {
                        foreach(RectTransform r in affected)
                            r.sizeDelta -= r.sizeDelta / denominator;
                    }
                }

                GUILayout.EndHorizontal();
                EditorGUI.EndChangeCheck();
            }

            ////Boundaries Stuff
            //GUILayout.Space(15);
            //EditorGUILayout.LabelField("Boundaries:", EditorStyles.boldLabel);
            //joystick.snapsToTouch = EditorGUILayout.Toggle("Snap to Touch", joystick.snapsToTouch);

            //EditorGUILayout.LabelField("Boundaries");
            //EditorGUIUtility.labelWidth = 15;
            //GUILayout.BeginHorizontal();
            //joystick.boundaries.x = EditorGUILayout.Slider("X", joystick.boundaries.x, 0, 1);
            //joystick.boundaries.y = EditorGUILayout.Slider("Y", joystick.boundaries.y, 0, 1);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //joystick.boundaries.width = EditorGUILayout.Slider("W", joystick.boundaries.width, 0, 1);
            //joystick.boundaries.height = EditorGUILayout.Slider("H", joystick.boundaries.height, 0, 1);
            //GUILayout.EndHorizontal();

            ////Bounds Anchor buttons
            //GUILayout.Space(3);
            //EditorGUILayout.LabelField("Bounds Anchor:", EditorStyles.boldLabel);
            //GUILayout.BeginHorizontal();
            //if (GUILayout.Button("Top Left", EditorStyles.miniButtonLeft))
            //{

            //}
            //if (GUILayout.Button("Top Right", EditorStyles.miniButtonRight))
            //{

            //}
            //GUILayout.EndHorizontal();

            //if (GUILayout.Button("Middle"))
            //{

            //}

            //GUILayout.BeginHorizontal();
            //if (GUILayout.Button("Bottom Left", EditorStyles.miniButtonLeft))
            //{

            //}
            //if (GUILayout.Button("Bottom Right", EditorStyles.miniButtonRight))
            //{

            //}
            //GUILayout.EndHorizontal();

            //if (EditorGUI.EndChangeCheck())
            //{

            //}
        }

        void OnSceneGUI() {

            VirtualJoystick vj = (VirtualJoystick)target;

            float radius = vj.GetRadius();

            // Draw the radius of the joystick.
            Handles.color = new Color(0, 1, 0, 0.1f);
            Handles.DrawSolidArc(vj.transform.position, Vector3.forward, Vector3.right, 360, radius);
            Handles.color = new Color(0, 1, 0, 0.5f);
            Handles.DrawWireArc(vj.transform.position, Vector3.forward, Vector3.right, 360, radius, 3f);
            
            // Draw the deadzone.
            Handles.color = new Color(1, 0, 0, 0.2f);
            Handles.DrawSolidArc(vj.transform.position, Vector3.forward, Vector3.right, 360, radius * vj.deadzone);
            Handles.color = new Color(1, 0, 0, 0.5f);
            Handles.DrawWireArc(vj.transform.position, Vector3.forward, Vector3.right, 360, radius * vj.deadzone, 3f);

            // Draw the boundaries of the joystick.
            if(vj.GetBounds().size.sqrMagnitude > 0) {
                // Draw the lines of the bounds.
                Handles.color = Color.yellow;

                // Get the 4 points in the bounds.
                Vector3 a = new Vector3(vj.boundaries.x, vj.boundaries.y),
                        b = new Vector3(vj.boundaries.x, vj.boundaries.y + Screen.height * vj.boundaries.height),
                        c = new Vector2(vj.boundaries.x + Screen.width * vj.boundaries.width, vj.boundaries.y + Screen.height * vj.boundaries.height),
                        d = new Vector3(vj.boundaries.x + Screen.width * vj.boundaries.width, vj.boundaries.y);
                Handles.DrawLine(a,b);
                Handles.DrawLine(b,c);
                Handles.DrawLine(c,d);
                Handles.DrawLine(d,a);
            }

            // Draw the direction anchors of the joystick.
            if(vj.directions > 0) {
                Handles.color = Color.blue;
                float partition = 360f / vj.directions;
                for(int i = 0; i < vj.directions;i++) {
                    Handles.DrawLine(vj.transform.position, vj.transform.position + Quaternion.Euler(0,0,i*partition + vj.angleOffset) * Vector2.right * radius, 2f);
                }
            }
        }

        // Function to return gcd of a and b
        int GCD(int a, int b) {
            if (b == 0) return a;
            return GCD(b, a % b);
        }

        // Function to find gcd of array of numbers
        int FindGCD(params int[] numbers) {
            if (numbers.Length == 0) {
                Debug.LogError("No numbers provided");
                return 0; // or handle the error in an appropriate way
            }

            int result = numbers[0];
            for (int i = 1; i < numbers.Length; i++) {

                result = GCD(result, numbers[i]);

                if (result == 1) {
                    return 1;
                } else if (result <= 0) {
                    Debug.LogError("The size value for one or more of the Joystick elements is not more than 0");
                    // You might want to handle this error in an appropriate way
                    return 0; // or handle the error in an appropriate way
                }
            }
            return result;
        }

        void RecordSizeChangeUndo(Object[] arguments) {
            for (int i = 0; i < arguments.Length; i++) {
                Undo.RecordObject(arguments[i], "Undo Virtual Joystick Size Change");
            }
        }
    }
}
