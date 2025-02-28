using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class TestEditor : EditorWindow
{
    private List<DialogSystem> dialogSystems = new List<DialogSystem>();
    private int selectedDialogIndex = 0;
    private float zoomLevel = 1f;
    private const float minZoom = 0.3f;
    private const float maxZoom = 3f;
    private DialogueLine selectedNode;
    private Vector2 panOffset;
    private Vector2 lastMousePosition;

    [MenuItem("Editor/Dialog")]
    public static TestEditor ShowWindow()
    {
        TestEditor window = GetWindow<TestEditor>();
        window.titleContent = new GUIContent("Dialog Tree Editor");
        window.minSize = new Vector2(300, 300);
        return window;
    }

    private void OnEnable()
    {
        LoadAllDialogSystems();
    }

    private void LoadAllDialogSystems()
    {
        dialogSystems.Clear();
        string[] guids = AssetDatabase.FindAssets("t:DialogSystem");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogSystem system = AssetDatabase.LoadAssetAtPath<DialogSystem>(path);
            if (system != null)
            {
                dialogSystems.Add(system);
            }
        }
    }

    private void OnGUI()
    {
        if (dialogSystems.Count == 0)
        {
            GUILayout.Label("Нет созданных диалоговых систем!", EditorStyles.boldLabel);
            if (GUILayout.Button("Создать новую систему"))
            {
                CreateNewDialogSystem();
            }
            return;
        }

        DialogSystem currentSystem = dialogSystems[selectedDialogIndex];

        HandleEvents(currentSystem);
        DrawGrid(20f, 0.1f, new Color(0.5f, 0.5f, 0.5f, 0.2f));
        DrawGrid(100f, 0.3f, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        if(currentSystem.dialogueLines == null)
        {
            return;
        }
        foreach (var node in currentSystem.dialogueLines)
        {
            DrawNode(node, currentSystem);
        }

        DrawConnections(currentSystem);

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        {
            DrawToolbar();
            Rect editorWindowRect = new Rect(
                0,
                0,
                position.width,
                position.height
            );
            GUI.BeginGroup(editorWindowRect);
            {
                Matrix4x4 oldMatrix = GUI.matrix;
                Matrix4x4 translation = Matrix4x4.TRS(
                    panOffset,
                    Quaternion.identity,
                    new Vector3(zoomLevel, zoomLevel, 1f)
                );

                GUI.matrix = translation;
                GUI.matrix = oldMatrix;
            }
            GUI.EndGroup();
        }
        EditorGUILayout.EndVertical();
    }
    private void CreateNewDialogSystem()
    {
        DialogSystem newSystem = CreateInstance<DialogSystem>();
        AssetDatabase.CreateAsset(newSystem, "Assets/Dialog/NewDialogSystem.asset");
        AssetDatabase.SaveAssets();
        LoadAllDialogSystems();
        selectedDialogIndex = dialogSystems.Count - 1;
    }

    private void HandleEvents(DialogSystem currentSystem)
    {
        Event e = Event.current;
        Rect canvasRect = new Rect(0, 0, position.width, position.height);

        // Панорамирование
        if (e.type == EventType.MouseDrag &&
           (e.button == 2 || (e.button == 0 && e.alt)))
        {
            panOffset += e.delta;
            Repaint();
        }

        // Масштабирование
        if (e.type == EventType.ScrollWheel && canvasRect.Contains(e.mousePosition))
        {
            float zoomChange = -e.delta.y * 0.01f;
            float oldZoom = zoomLevel;
            zoomLevel = Mathf.Clamp(zoomLevel + zoomChange, minZoom, maxZoom);

            Vector2 mouseWorldPosBefore = ScreenToWorld(e.mousePosition);
            Vector2 mouseWorldPosAfter = ScreenToWorld(e.mousePosition);
            panOffset += (mouseWorldPosAfter - mouseWorldPosBefore) * zoomLevel;

            Repaint();
        }

        // Перемещение узлов
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            lastMousePosition = ScreenToWorld(e.mousePosition);
            selectedNode = null;
            if(currentSystem.dialogueLines == null)
            {
                return;
            }
            foreach (var node in currentSystem.dialogueLines)
            {
                Rect nodeRect = GetNodeRect(node.nodePosition);
                if (nodeRect.Contains(e.mousePosition))
                {
                    selectedNode = node;
                    break;
                }
            }   
        }

        if (e.type == EventType.MouseDrag && selectedNode != null)
        {
            Vector2 currentMouseWorldPos = ScreenToWorld(e.mousePosition);
            Vector2 delta = currentMouseWorldPos - lastMousePosition;
            selectedNode.nodePosition += delta;
            lastMousePosition = currentMouseWorldPos;
            Repaint();
        }
    }

    private Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return (screenPos - panOffset) / zoomLevel;
    }

    private void DrawNode(DialogueLine node, DialogSystem currentSystem)
    {
        float nodeWidth = Mathf.Lerp(10, 400, zoomLevel);
        float nodeHeight = Mathf.Lerp(10, 300, zoomLevel);

        Vector2 nodePosition = (node.nodePosition * zoomLevel) + panOffset;

        Rect nodeRect = new Rect(
            nodePosition.x,
            nodePosition.y,
            nodeWidth,
            nodeHeight
        );

        GUIStyle nodeStyle = new GUIStyle("box")
        {
            normal = { background = MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 0.8f)) },
            padding = new RectOffset(10, 10, 10, 10)
        };

        GUI.Box(nodeRect, GUIContent.none, nodeStyle);

        if (nodeRect.Contains(Event.current.mousePosition) &&
            Event.current.type == EventType.ScrollWheel)
        {
            node.scrollPosition.y += Event.current.delta.y * 20;
            Event.current.Use();
            Repaint();
        }

        GUILayout.BeginArea(nodeRect);
        {
            node.scrollPosition = GUILayout.BeginScrollView(node.scrollPosition);

            EditorGUIUtility.labelWidth = 70 * zoomLevel;
            EditorGUIUtility.fieldWidth = 130 * zoomLevel;

            EditorGUILayout.LabelField($"Узел {System.Array.IndexOf(currentSystem.dialogueLines, node) + 1}",
                EditorStyles.boldLabel);

            node.textDialogue = EditorGUILayout.TextArea(node.textDialogue,
                GUILayout.Height(40 * zoomLevel));

            node.character = (NameNPC)EditorGUILayout.ObjectField("NPC", node.character,
                typeof(NameNPC), false,
                GUILayout.Height(20 * zoomLevel));

            node.indexSprite = EditorGUILayout.IntField("Индекс спрайта", node.indexSprite,
                GUILayout.Height(20 * zoomLevel));

            node.background = (Sprite)EditorGUILayout.ObjectField("Задний фон", node.background,
                typeof(Sprite), false,
                GUILayout.Height(60 * zoomLevel));

            EditorGUILayout.Space(10 * zoomLevel);

            if (node.choices != null)
            {
                for (int i = 0; i < node.choices.Length; i++)
                {
                    DrawChoice(node, i, currentSystem);
                }
            }

            if (GUILayout.Button("Добавить выбор", GUILayout.Height(25 * zoomLevel)))
            {
                AddNewChoice(node);
            }

            if (GUILayout.Button("Удалить узел", GUILayout.Height(25 * zoomLevel)))
            {
                RemoveNode(node, currentSystem);
            }

            GUILayout.EndScrollView();
        }
        GUILayout.EndArea();
    }


    private void DrawChoice(DialogueLine node, int choiceIndex, DialogSystem currentSystem)
    {
        DialogueChoice choice = node.choices[choiceIndex];

        EditorGUILayout.BeginVertical("box");
        {
            choice.choiceText = EditorGUILayout.TextField(choice.choiceText);

            List<string> options = new List<string> { "Никто" };
            foreach (var n in currentSystem.dialogueLines)
            {
                options.Add($"Узел {System.Array.IndexOf(currentSystem.dialogueLines, n) + 1}");
            }

            int selected = choice.nextLineIndex + 1;
            selected = EditorGUILayout.Popup("Следующий узел", selected, options.ToArray());
            choice.nextLineIndex = selected - 1;

            if (GUILayout.Button("Удалить выбор"))
            {
                RemoveChoice(node, choiceIndex);
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawConnections(DialogSystem system)
    {
        foreach (var node in system.dialogueLines)
        {
            if (node.choices != null)
            {
                foreach (var choice in node.choices)
                {
                    if (choice.nextLineIndex >= 0 && choice.nextLineIndex < system.dialogueLines.Length)
                    {
                        DialogueLine nextNode = system.dialogueLines[choice.nextLineIndex];
                        DrawNodeCurve(GetNodeRect(node.nodePosition), GetNodeRect(nextNode.nodePosition));
                    }
                }
            }
        }
    }

    private void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 3f);
    }

    private Rect GetNodeRect(Vector2 position)
    {
        return new Rect(
            (position.x * zoomLevel) + panOffset.x,
            (position.y * zoomLevel) + panOffset.y,
            300 * zoomLevel,
            300 * zoomLevel
        );
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            // Выбор системы диалогов
            string[] options = new string[dialogSystems.Count];
            for (int i = 0; i < dialogSystems.Count; i++)
            {
                options[i] = dialogSystems[i].name;
            }

            selectedDialogIndex = EditorGUILayout.Popup(
                selectedDialogIndex,
                options,
                GUILayout.Width(200)
            );

            if (GUILayout.Button("Обновить список", EditorStyles.toolbarButton))
            {
                LoadAllDialogSystems();
            }

            if (GUILayout.Button("Новая система", EditorStyles.toolbarButton))
            {
                CreateNewDialogSystem();
            }

            if (GUILayout.Button("Добавить узел", EditorStyles.toolbarButton))
            {
                AddNewNode();
            }

            if (GUILayout.Button("Сохранить", EditorStyles.toolbarButton))
            {
                SaveCurrentSystem();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    private void SaveCurrentSystem()
    {
        if (dialogSystems.Count > selectedDialogIndex)
        {
            EditorUtility.SetDirty(dialogSystems[selectedDialogIndex]);
            AssetDatabase.SaveAssets();
        }
    }
    private void AddNewNode()
    {
        if (dialogSystems.Count == 0) return;

        DialogSystem currentSystem = dialogSystems[selectedDialogIndex];
        List<DialogueLine> nodes = new List<DialogueLine>(currentSystem.dialogueLines);
        DialogueLine newNode = new DialogueLine();
        newNode.nodePosition = new Vector2(100 + nodes.Count * 350, 100);
        nodes.Add(newNode);
        currentSystem.dialogueLines = nodes.ToArray();
        SaveCurrentSystem();
    }

    private void RemoveNode(DialogueLine node, DialogSystem dialogSystem)
    {
        List<DialogueLine> nodes = new List<DialogueLine>(dialogSystem.dialogueLines);
        nodes.Remove(node);
        dialogSystem.dialogueLines = nodes.ToArray();
    }

    private void AddNewChoice(DialogueLine node)
    {
        List<DialogueChoice> choices = new List<DialogueChoice>(node.choices ?? new DialogueChoice[0]);
        choices.Add(new DialogueChoice());
        node.choices = choices.ToArray();
    }

    private void RemoveChoice(DialogueLine node, int index)
    {
        List<DialogueChoice> choices = new List<DialogueChoice>(node.choices);
        choices.RemoveAt(index);
        node.choices = choices.ToArray();
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        // Рассчитываем размеры с учетом текущего окна
        float width = position.width / zoomLevel;
        float height = position.height / zoomLevel;

        // Начальные координаты с учетом панорамирования
        float startX = (panOffset.x % gridSpacing) - gridSpacing;
        float startY = (panOffset.y % gridSpacing) - gridSpacing;

        // Количество линий для полного покрытия
        int verticalLines = Mathf.CeilToInt(width / gridSpacing) + 2;
        int horizontalLines = Mathf.CeilToInt(height / gridSpacing) + 2;

        // Вертикальные линии
        for (int x = 0; x < verticalLines; x++)
        {
            float xPos = startX + x * gridSpacing;
            Handles.DrawLine(
                new Vector3(xPos, -height, 0),
                new Vector3(xPos, height * 2, 0)
            );
        }

        // Горизонтальные линии
        for (int y = 0; y < horizontalLines; y++)
        {
            float yPos = startY + y * gridSpacing;
            Handles.DrawLine(
                new Vector3(-width, yPos, 0),
                new Vector3(width * 2, yPos, 0)
            );
        }
        Handles.EndGUI();
    }
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
