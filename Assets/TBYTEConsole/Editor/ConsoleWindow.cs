using UnityEngine;
using UnityEditor;
using System.Collections;

namespace TBYTEConsole
{
    public class ConsoleWindow : EditorWindow
    {
        string consoleDisplay;
        Vector2 consoleScrollPos;

        string userEntry;
        bool refocusState = false;

        [MenuItem("TBYTEConsole/Show Window")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ConsoleWindow));
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Console");
        }

        // TODO: prevent people from editting the display, but retain highlight
        void OnGUI()
        {
            
            

            GUIStyle textStyle = new GUIStyle(EditorStyles.textField);
            Vector2 textSize = textStyle.CalcSize(new GUIContent(consoleDisplay));

            // Display
            consoleScrollPos = EditorGUILayout.BeginScrollView(consoleScrollPos);

                EditorGUILayout.SelectableLabel(consoleDisplay, textStyle,
                                                GUILayout.ExpandHeight(true),
                                                GUILayout.ExpandWidth(true),
                                                GUILayout.MinWidth(textSize.x),
                                                GUILayout.MinHeight(textSize.y));

            EditorGUILayout.EndScrollView();

            // Input
            bool refocusWish = false;

            Event e = Event.current;
            if (Event.current.type == EventType.KeyDown &&
                e.keyCode == KeyCode.Return)
            {
                consoleScrollPos = new Vector2(0, Mathf.Infinity);
                consoleDisplay = Console.ProcessConsoleInput(userEntry);
                userEntry = string.Empty;
                Repaint();

                refocusWish = true;
                EditorGUIUtility.keyboardControl = 0;
            }

            int userEntryControlID = EditorGUIUtility.GetControlID(FocusType.Keyboard) + 1;

            GUI.SetNextControlName("TBYTEConsole.ConsoleWindow.userEntry");
            userEntry = EditorGUILayout.TextField(userEntry);

            // TODO: Make refocusing actually work
            if (refocusState)
            {
                EditorGUIUtility.keyboardControl = userEntryControlID;
                if (EditorGUIUtility.keyboardControl == userEntryControlID)
                {
                    refocusWish = refocusState = false;
                }
                else
                {
                    refocusWish = true;
                }
            }

            refocusState = refocusWish;
        }
    }
}
