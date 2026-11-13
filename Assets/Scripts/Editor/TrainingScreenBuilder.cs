using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 육성 화면 UI를 코드로 구성하는 에디터 도구 (Docs/UI_육성화면.md 2장 레이아웃)
//
//   Tools > UI > 육성 화면 구성
//
// 열려 있는 씬의 Canvas 아래 UI를 모두 지우고 3열 레이아웃(상태 / 일과 / 성장) + 상단 바 + 하단 로그를 새로 만든 뒤,
// UIManager · KeyboardInputManager 등 씬의 시스템 오브젝트에 참조를 연결한다. 여러 번 실행해도 결과가 같다.
// 실행 후 Ctrl+Z로 되돌릴 수 있고, 씬 저장은 직접 한다.
public static class TrainingScreenBuilder
{
    private const string FontPath = "Assets/Fonts/NotoSansKR SDF.asset";
    private const string CatalogPath = "Assets/ScriptableObjects/행동 카탈로그.asset";
    private const string LogLinePrefabPath = "Assets/Prefabs/UI/LogLinePrefab.prefab";
    private const string RoutineCardPrefabPath = "Assets/Prefabs/UI/RoutineCard.prefab";

    // 레이아웃 수치 (1920×1080 기준)
    private const float TopBarHeight = 72f;
    private const float LogCollapsedHeight = 48f;
    private const float LogExpandedHeight = 240f;
    private const float SideColumnWidth = 520f;
    private const float Gutter = 16f;

    // 색
    private static readonly Color ScreenBg = Hex("#121418");
    private static readonly Color PanelBg = Hex("#1B1E24");
    private static readonly Color LogBg = Hex("#16191E");
    private static readonly Color DividerColor = Hex("#2C3038");
    private static readonly Color BarBg = Hex("#2A2E36");
    private static readonly Color TextMain = Hex("#EBEBEB");
    private static readonly Color TextSub = Hex("#9AA0A8");
    private static readonly Color Accent = Hex("#E0B050");
    private static readonly Color ButtonBg = Hex("#8A6A28");

    private static TMP_FontAsset _font;

    [MenuItem("Tools/UI/육성 화면 구성")]
    public static void Build() {
        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        var catalog = AssetDatabase.LoadAssetAtPath<ActionCatalog>(CatalogPath);
        var logLinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LogLinePrefabPath);
        var canvas = FindCanvas();
        var uiManager = Object.FindFirstObjectByType<UIManager>();
        var keyboard = Object.FindFirstObjectByType<KeyboardInputManager>();

        if(_font == null || catalog == null || logLinePrefab == null || canvas == null || uiManager == null) {
            EditorUtility.DisplayDialog("육성 화면 구성",
                "필요한 애셋이나 씬 오브젝트를 찾지 못했습니다.\n\n" +
                $"폰트: {Ok(_font)}\n행동 카탈로그: {Ok(catalog)}\n로그 줄 프리팹: {Ok(logLinePrefab)}\n" +
                $"Canvas: {Ok(canvas)}\nUIManager: {Ok(uiManager)}", "확인");
            return;
        }

        if(!EditorUtility.DisplayDialog("육성 화면 구성",
               $"'{canvas.name}' 아래의 기존 UI를 모두 지우고 육성 화면을 새로 구성합니다.\n" +
               $"일과 카드 프리팹({RoutineCardPrefabPath})도 다시 만듭니다.\n\n" +
               "Ctrl+Z로 되돌릴 수 있습니다. 씬 저장은 직접 해주세요.", "구성", "취소")) {
            return;
        }

        var cardPrefab = BuildRoutineCardPrefab();

        Undo.SetCurrentGroupName("육성 화면 구성");
        int undoGroup = Undo.GetCurrentGroup();

        // 기존 UI 제거
        for(int i = canvas.transform.childCount - 1; i >= 0; i--) {
            Undo.DestroyObjectImmediate(canvas.transform.GetChild(i).gameObject);
        }

        // 루트
        var screen = NewUI("TrainingScreen", canvas.transform);
        Stretch(screen);
        AddImage(screen, ScreenBg);

        var topBar = BuildTopBar(screen, out var turnInfoText, out var resourceBar);

        var body = NewUI("Body", screen);
        Stretch(body, Gutter, LogCollapsedHeight + Gutter, Gutter, TopBarHeight + Gutter);

        var status = BuildStatusColumn(body);
        var growth = BuildGrowthColumn(body);
        var routine = BuildRoutineColumn(body, catalog, cardPrefab, out var endTurnButton, out var endTurnHint);
        var log = BuildLogPanel(screen, logLinePrefab);
        Undo.RegisterCreatedObjectUndo(screen.gameObject, "육성 화면 구성"); // 하위 구성을 마친 뒤 한 번에 등록

        // 시스템 오브젝트 연결
        var ui = new SerializedObject(uiManager);
        Set(ui, "turnInfoText", turnInfoText);
        Set(ui, "endTurnButton", endTurnButton);
        Set(ui, "endTurnHintText", endTurnHint);
        ui.ApplyModifiedProperties();

        if(keyboard != null) {
            var kb = new SerializedObject(keyboard);
            var toggles = kb.FindProperty("panelToggles");
            toggles.arraySize = 1;
            var entry = toggles.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("key").intValue = (int)Key.Backquote;
            entry.FindPropertyRelative("panel").objectReferenceValue = log;
            kb.ApplyModifiedProperties();
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Selection.activeGameObject = screen.gameObject;
        Debug.Log($"[육성 화면 구성] 완료: 상단 바 · 상태({status.name}) · 일과({routine.name}) · 성장({growth.name}) · 로그({log.name}). 씬을 저장하세요.");
    }

    // ───────────────────────── 상단 바 ─────────────────────────

    private static RectTransform BuildTopBar(RectTransform parent, out TextMeshProUGUI turnInfo, out ResourceBar resourceBar) {
        var bar = NewUI("TopBar", parent);
        Anchor(bar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        bar.sizeDelta = new Vector2(0, TopBarHeight);
        AddImage(bar, PanelBg);

        turnInfo = Text(bar, "TurnInfoText", "1턴 · 계획 단계", 28, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        var tr = turnInfo.rectTransform;
        Anchor(tr, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
        tr.sizeDelta = new Vector2(600, 0);
        tr.anchoredPosition = new Vector2(24, 0);

        var res = NewUI("ResourceBar", bar);
        Anchor(res, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        res.sizeDelta = new Vector2(720, 0);
        res.anchoredPosition = new Vector2(-24, 0);
        var h = res.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.childAlignment = TextAnchor.MiddleRight;
        h.spacing = 10;
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = false; h.childForceExpandHeight = true;

        TextMeshProUGUI ResourcePair(string label, string sample, bool last) {
            Text(res, $"{label}Label", label, 22, TextAlignmentOptions.MidlineRight, color: TextSub);
            var v = Text(res, $"{label}Value", sample, 24, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            if(!last) v.margin = new Vector4(0, 0, 28, 0);
            return v;
        }
        var gold = ResourcePair("골드", "1,000", false);
        var supply = ResourcePair("보급", "50", false);
        var influence = ResourcePair("영향력", "10", true);

        resourceBar = res.gameObject.AddComponent<ResourceBar>();
        var so = new SerializedObject(resourceBar);
        Set(so, "goldText", gold);
        Set(so, "supplyText", supply);
        Set(so, "influenceText", influence);
        so.ApplyModifiedPropertiesWithoutUndo();

        return bar;
    }

    // ───────────────────────── ② 용병 상태 (좌) ─────────────────────────

    private static MercenaryStatusPanel BuildStatusColumn(RectTransform body) {
        var col = NewUI("StatusColumn", body);
        Anchor(col, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
        col.sizeDelta = new Vector2(SideColumnWidth, 0);
        AddImage(col, PanelBg);
        var v = VLayout(col, 28, 12);

        var nameText = Text(col, "NameText", "용병 이름", 40, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        Divider(col);

        var staminaText = Text(col, "StaminaText", "기력 100 · 양호", 22, TextAlignmentOptions.MidlineLeft);
        var staminaBar = Bar(col, "StaminaBar", 16);
        Spacer(col, 6);
        var hpText = Text(col, "HpText", "HP 100 / 100", 22, TextAlignmentOptions.MidlineLeft);
        var hpBar = Bar(col, "HpBar", 16);

        Divider(col);
        Text(col, "StatsHeader", "능력치", 20, TextAlignmentOptions.MidlineLeft, color: TextSub);

        var panel = col.gameObject.AddComponent<MercenaryStatusPanel>();
        var so = new SerializedObject(panel);

        void StatRow(string field, string label) {
            var row = NewUI($"{field}Row", col);
            Layout(row, preferredHeight: 32);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 12;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;

            var l = Text(row, "Label", label, 22, TextAlignmentOptions.MidlineLeft);
            Layout(l.rectTransform, preferredWidth: 80, preferredHeight: 32);
            var bar = Bar(row, "Bar", 14);
            Layout(bar.GetComponent<RectTransform>(), preferredHeight: 14, flexibleWidth: 1);
            var val = Text(row, "Value", "0", 22, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
            Layout(val.rectTransform, preferredWidth: 40, preferredHeight: 32);

            Set(so, $"{field}.bar", bar);
            Set(so, $"{field}.value", val);
        }
        StatRow("strength", "힘");
        StatRow("agility", "민첩");
        StatRow("health", "건강");
        StatRow("intelligence", "지능");
        StatRow("charming", "매력");

        Divider(col);
        var weaponText = Text(col, "WeaponText", "무기  -", 22, TextAlignmentOptions.MidlineLeft);
        var gripText = Text(col, "GripText", "운용  -", 22, TextAlignmentOptions.MidlineLeft);

        Set(so, "nameText", nameText);
        Set(so, "staminaBar", staminaBar);
        Set(so, "staminaText", staminaText);
        Set(so, "hpBar", hpBar);
        Set(so, "hpText", hpText);
        Set(so, "weaponText", weaponText);
        Set(so, "gripText", gripText);
        so.ApplyModifiedPropertiesWithoutUndo();
        return panel;
    }

    // ───────────────────────── ③ 일과 선택 (중앙) ─────────────────────────

    private static RoutinePanel BuildRoutineColumn(RectTransform body, ActionCatalog catalog, RoutineCard cardPrefab,
                                                   out Button endTurnButton, out TextMeshProUGUI endTurnHint) {
        var col = NewUI("RoutineColumn", body);
        Stretch(col, SideColumnWidth + Gutter, 0, SideColumnWidth + Gutter, 0);
        AddImage(col, PanelBg);
        VLayout(col, 32, 20);

        var title = Text(col, "Title", "오늘의 일과", 30, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        Layout(title.rectTransform, preferredHeight: 44);

        var grid = NewUI("CardGrid", col);
        Layout(grid, flexibleHeight: 1);
        var g = grid.gameObject.AddComponent<GridLayoutGroup>();
        g.cellSize = new Vector2(360, 190);
        g.spacing = new Vector2(24, 24);
        g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        g.constraintCount = 2;
        g.childAlignment = TextAnchor.UpperCenter;

        var selection = Text(col, "SelectionText", "일과를 선택하세요", 22, TextAlignmentOptions.Center);
        Layout(selection.rectTransform, preferredHeight: 36);

        var row = NewUI("EndTurnRow", col);
        Layout(row, preferredHeight: 72);
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 20;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = false; h.childForceExpandHeight = false;

        var btnRt = NewUI("EndTurnButton", row);
        Layout(btnRt, preferredWidth: 340, preferredHeight: 72);
        var btnImg = AddImage(btnRt, ButtonBg);
        endTurnButton = btnRt.gameObject.AddComponent<Button>();
        endTurnButton.targetGraphic = btnImg;
        endTurnButton.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = endTurnButton.colors;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        endTurnButton.colors = colors;
        var btnText = Text(btnRt, "Label", "턴 넘기기", 30, TextAlignmentOptions.Center, FontStyles.Bold);
        Stretch(btnText.rectTransform);

        endTurnHint = Text(row, "HintText", "일과를 선택하세요", 20, TextAlignmentOptions.MidlineLeft, color: TextSub);
        Layout(endTurnHint.rectTransform, preferredWidth: 220, preferredHeight: 72);

        var panel = col.gameObject.AddComponent<RoutinePanel>();
        var so = new SerializedObject(panel);
        Set(so, "actionCatalog", catalog);
        Set(so, "cardContainer", grid);
        Set(so, "cardPrefab", cardPrefab);
        Set(so, "selectionText", selection);
        so.ApplyModifiedPropertiesWithoutUndo();
        return panel;
    }

    // 일과 카드 프리팹 (360×190)
    private static RoutineCard BuildRoutineCardPrefab() {
        var root = new GameObject("RoutineCard", typeof(RectTransform));
        try {
            var rt = (RectTransform)root.transform;
            rt.sizeDelta = new Vector2(360, 190);
            var bg = AddImage(rt, Hex("#2B3039"));

            var button = root.AddComponent<Button>();
            button.targetGraphic = bg;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            button.colors = colors;

            var outline = root.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(3, -3);
            outline.enabled = false;

            var name = Text(rt, "NameText", "일과", 30, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            Anchor(name.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
            name.rectTransform.offsetMin = new Vector2(20, -60);
            name.rectTransform.offsetMax = new Vector2(-70, -16);

            var hotkey = Text(rt, "HotkeyText", "[1]", 20, TextAlignmentOptions.TopRight, color: TextSub);
            Anchor(hotkey.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
            hotkey.rectTransform.sizeDelta = new Vector2(60, 30);
            hotkey.rectTransform.anchoredPosition = new Vector2(-16, -18);

            var desc = Text(rt, "DescriptionText", "", 19, TextAlignmentOptions.TopLeft);
            Stretch(desc.rectTransform, 20, 48, 20, 66);

            var state = Text(rt, "StateText", "", 20, TextAlignmentOptions.BottomLeft, FontStyles.Bold, Accent);
            Anchor(state.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0));
            state.rectTransform.offsetMin = new Vector2(20, 14);
            state.rectTransform.offsetMax = new Vector2(-20, 44);

            var card = root.AddComponent<RoutineCard>();
            var so = new SerializedObject(card);
            Set(so, "button", button);
            Set(so, "background", bg);
            Set(so, "selectedOutline", outline);
            Set(so, "nameText", name);
            Set(so, "hotkeyText", hotkey);
            Set(so, "descriptionText", desc);
            Set(so, "stateText", state);
            so.ApplyModifiedPropertiesWithoutUndo();

            var saved = PrefabUtility.SaveAsPrefabAsset(root, RoutineCardPrefabPath);
            return saved.GetComponent<RoutineCard>();
        } finally {
            Object.DestroyImmediate(root);
        }
    }

    // ───────────────────────── ④ 성장 정보 (우) ─────────────────────────

    private static GrowthPanel BuildGrowthColumn(RectTransform body) {
        var col = NewUI("GrowthColumn", body);
        Anchor(col, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        col.sizeDelta = new Vector2(SideColumnWidth, 0);
        AddImage(col, PanelBg);

        var tabBar = NewUI("TabBar", col);
        Anchor(tabBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        tabBar.offsetMin = new Vector2(20, -76);
        tabBar.offsetMax = new Vector2(-20, -20);
        var h = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 8;
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = true; h.childForceExpandHeight = true;

        var contentArea = NewUI("TabContent", col);
        Stretch(contentArea, 28, 24, 28, 96);

        var panel = col.gameObject.AddComponent<GrowthPanel>();
        var so = new SerializedObject(panel);
        var tabs = so.FindProperty("tabs");
        tabs.arraySize = 3;

        string[] names = { "숙련", "스킬", "장비" };
        string[] fields = { "proficiencyText", "skillText", "equipmentText" };
        for(int i = 0; i < 3; i++) {
            var tabRt = NewUI($"Tab_{names[i]}", tabBar);
            var tabImg = AddImage(tabRt, Hex("#2B3039"));
            var tabBtn = tabRt.gameObject.AddComponent<Button>();
            tabBtn.targetGraphic = tabImg;
            tabBtn.navigation = new Navigation { mode = Navigation.Mode.None };
            var label = Text(tabRt, "Label", names[i], 22, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(label.rectTransform);

            var content = NewUI($"Content_{names[i]}", contentArea);
            Stretch(content);
            var text = Text(content, "Text", "", 22, TextAlignmentOptions.TopLeft);
            Stretch(text.rectTransform);
            text.lineSpacing = 12;

            var entry = tabs.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("button").objectReferenceValue = tabBtn;
            entry.FindPropertyRelative("buttonBackground").objectReferenceValue = tabImg;
            entry.FindPropertyRelative("content").objectReferenceValue = content.gameObject;
            Set(so, fields[i], text);
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        return panel;
    }

    // ───────────────────────── ⑤ 로그 (하단) ─────────────────────────

    private static ActionLogPanel BuildLogPanel(RectTransform screen, GameObject logLinePrefab) {
        var panelRt = NewUI("LogPanel", screen);
        Anchor(panelRt, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        panelRt.sizeDelta = new Vector2(0, LogCollapsedHeight);
        AddImage(panelRt, LogBg);

        // 머리줄: "로그" + 최근 한 줄 + 펼치기 버튼
        var header = NewUI("Header", panelRt);
        Anchor(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        header.sizeDelta = new Vector2(0, LogCollapsedHeight);

        var title = Text(header, "Title", "로그", 20, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, TextSub);
        Anchor(title.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
        title.rectTransform.sizeDelta = new Vector2(60, 0);
        title.rectTransform.anchoredPosition = new Vector2(24, 0);

        var latest = Text(header, "LatestLineText", "", 20, TextAlignmentOptions.MidlineLeft);
        Stretch(latest.rectTransform, 96, 0, 200, 0);
        latest.textWrappingMode = TextWrappingModes.NoWrap;
        latest.overflowMode = TextOverflowModes.Ellipsis;

        var toggleRt = NewUI("ToggleButton", header);
        Anchor(toggleRt, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        toggleRt.sizeDelta = new Vector2(160, -12);
        toggleRt.anchoredPosition = new Vector2(-16, 0);
        var toggleImg = AddImage(toggleRt, Hex("#2B3039"));
        var toggleBtn = toggleRt.gameObject.AddComponent<Button>();
        toggleBtn.targetGraphic = toggleImg;
        toggleBtn.navigation = new Navigation { mode = Navigation.Mode.None };
        var hint = Text(toggleRt, "Label", "[`] 펼치기", 18, TextAlignmentOptions.Center, color: TextSub);
        Stretch(hint.rectTransform);

        // 펼쳤을 때의 목록
        var scrollRt = NewUI("ScrollView", panelRt);
        Stretch(scrollRt, 24, 8, 24, LogCollapsedHeight);
        var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30;

        var viewport = NewUI("Viewport", scrollRt);
        Stretch(viewport);
        viewport.gameObject.AddComponent<RectMask2D>();

        var content = NewUI("Content", viewport);
        Anchor(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        content.sizeDelta = Vector2.zero;
        var v = content.gameObject.AddComponent<VerticalLayoutGroup>();
        v.childControlWidth = true; v.childControlHeight = false; // 로그 줄 프리팹은 자기 높이(32)를 쓴다
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;

        var panel = panelRt.gameObject.AddComponent<ActionLogPanel>();
        var so = new SerializedObject(panel);
        Set(so, "logContainer", content);
        Set(so, "logLinePrefab", logLinePrefab);
        Set(so, "scrollRect", scroll);
        Set(so, "panelRect", panelRt);
        Set(so, "expandedContent", scrollRt.gameObject);
        Set(so, "latestLineText", latest);
        Set(so, "toggleHintText", hint);
        Set(so, "toggleButton", toggleBtn);
        so.FindProperty("collapsedHeight").floatValue = LogCollapsedHeight;
        so.FindProperty("expandedHeight").floatValue = LogExpandedHeight;
        so.ApplyModifiedPropertiesWithoutUndo();
        return panel;
    }

    // ───────────────────────── 헬퍼 ─────────────────────────

    private static Canvas FindCanvas() {
        Canvas fallback = null;
        foreach(var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) {
            if(!c.isRootCanvas) continue;
            if(c.name == "Canvas") return c;
            fallback ??= c;
        }
        return fallback;
    }

    private static RectTransform NewUI(string name, Transform parent) {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot) {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
    }

    // 부모를 꽉 채운다 (left, bottom, right, top 여백)
    private static void Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0) {
        Anchor(rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static Image AddImage(RectTransform rt, Color color) {
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private static VerticalLayoutGroup VLayout(RectTransform rt, int padding, float spacing) {
        var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(padding, padding, padding, padding);
        v.spacing = spacing;
        v.childAlignment = TextAnchor.UpperLeft;
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        return v;
    }

    private static LayoutElement Layout(RectTransform rt, float preferredWidth = -1, float preferredHeight = -1,
                                        float flexibleWidth = -1, float flexibleHeight = -1) {
        if(!rt.TryGetComponent(out LayoutElement le)) le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = preferredWidth;
        le.preferredHeight = preferredHeight;
        le.flexibleWidth = flexibleWidth;
        le.flexibleHeight = flexibleHeight;
        return le;
    }

    private static void Divider(RectTransform parent) {
        var d = NewUI("Divider", parent);
        AddImage(d, DividerColor);
        Layout(d, preferredHeight: 2);
    }

    private static void Spacer(RectTransform parent, float height) {
        Layout(NewUI("Spacer", parent), preferredHeight: height);
    }

    private static TextMeshProUGUI Text(RectTransform parent, string name, string text, float size,
                                        TextAlignmentOptions align, FontStyles style = FontStyles.Normal, Color? color = null) {
        var rt = NewUI(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = _font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color ?? TextMain;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        return t;
    }

    // 조작 불가 막대 (기력·HP·능력치)
    private static Slider Bar(RectTransform parent, string name, float height) {
        var rt = NewUI(name, parent);
        Layout(rt, preferredHeight: height);

        var bg = NewUI("Background", rt);
        Stretch(bg);
        AddImage(bg, BarBg);

        var fillArea = NewUI("Fill Area", rt);
        Stretch(fillArea);
        var fill = NewUI("Fill", fillArea);
        Stretch(fill);
        AddImage(fill, Hex("#6BB373"));

        var slider = rt.gameObject.AddComponent<Slider>();
        slider.fillRect = fill;
        slider.handleRect = null;
        slider.direction = Slider.Direction.LeftToRight;
        slider.transition = Selectable.Transition.None;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
        slider.interactable = false;
        slider.wholeNumbers = true;
        slider.minValue = 0;
        slider.maxValue = 10;
        slider.value = 5;
        return slider;
    }

    private static void Set(SerializedObject so, string path, Object value) {
        var p = so.FindProperty(path);
        if(p == null) {
            Debug.LogError($"[육성 화면 구성] {so.targetObject.GetType().Name}에 '{path}' 필드가 없습니다.");
            return;
        }
        p.objectReferenceValue = value;
    }

    private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    private static string Ok(Object o) => o != null ? "있음" : "없음";
}
