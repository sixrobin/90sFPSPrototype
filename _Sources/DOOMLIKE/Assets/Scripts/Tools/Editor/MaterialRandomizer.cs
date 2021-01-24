namespace RSLib
{
    using RSLib.Extensions;
    using UnityEditor;
    using UnityEngine;

    internal sealed class MaterialRandomizerMenu
    {
        [MenuItem("Tools/Material Randomizer")]
        public static void RenameSelectedObjects()
        {
            MaterialRandomizerEditor.LaunchRenamer();
        }
    }

    internal sealed class MaterialRandomizerEditor : EditorWindow
    {
        private GameObject[] _selection;
        private string _filter;
        private int _materialsLength;
        private int _materialsToSkipLength;
        private int _previousLength;
        private int _previousToSkipLength;
        private Object[] _materials;
        private Object[] _materialsToSkip;
        private System.Collections.Generic.List<Material> _materialsToSkipList;
        private System.Collections.Generic.List<Material> _rndMaterials;

        public delegate void ChildEventHandler(GameObject child);

        public static void LaunchRenamer()
        {
            GetWindow<MaterialRandomizerEditor>("Material Randomizer").Show();
        }

        private void RandomizeMaterial(GameObject go)
        {
            if (go.name.ToLower().Contains(_filter.ToLower())
                && go.TryGetComponent(out MeshRenderer meshRenderer)
                && !_materialsToSkipList.Contains(meshRenderer.sharedMaterial))
                meshRenderer.material = _rndMaterials.Any() as Material;
        }

        public static void IterateChildren(GameObject gameObject, ChildEventHandler childHandler, bool recursive = true)
        {
            DoIterate(gameObject, childHandler, recursive);
        }

        private static void DoIterate(GameObject gameObject, ChildEventHandler childHandler, bool recursive = true)
        {
            childHandler(gameObject);

            foreach (Transform child in gameObject.transform)
            {
                childHandler(child.gameObject);
                if (recursive)
                    DoIterate(child.gameObject, childHandler);
            }
        }

        private void OnGUI()
        {
            _selection = Selection.gameObjects;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            EditorGUILayout.LabelField("SELECTED : " + _selection.Length, EditorStyles.boldLabel);

            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);

            _filter = EditorGUILayout.TextField("Filter : ", _filter, EditorStyles.miniTextField, GUILayout.ExpandWidth(true));

            _materialsLength = EditorGUILayout.IntField("Materials Length : ", _materialsLength, EditorStyles.miniTextField, GUILayout.ExpandWidth(true));

            if (_materials != null && _previousLength != _materialsLength)
            {
                Object[] copy = new Object[_materials.Length];
                System.Array.Copy(_materials, copy, _materials.Length);
                _materials = new Object[_materialsLength];

                for (int i = 0; i < _materials.Length; ++i)
                    if (i < copy.Length)
                        _materials[i] = copy[i];

                _previousLength = _materialsLength;
            }

            if (_materials == null)
                _materials = new Object[_materialsLength];

            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);
            for (int i = 0; i < _materialsLength; ++i)
                _materials[i] = EditorGUILayout.ObjectField(_materials[i], typeof(Material), true);

            EditorGUILayout.Space(15f);

            _materialsToSkipLength = EditorGUILayout.IntField("Materials to Skip Length : ", _materialsToSkipLength, EditorStyles.miniTextField, GUILayout.ExpandWidth(true));

            if (_materialsToSkip != null && _previousToSkipLength != _materialsToSkipLength)
            {
                Object[] copy = new Object[_materialsToSkip.Length];
                System.Array.Copy(_materialsToSkip, copy, _materialsToSkip.Length);
                _materialsToSkip = new Object[_materialsToSkipLength];

                for (int i = 0; i < _materialsToSkip.Length; ++i)
                    if (i < copy.Length)
                        _materialsToSkip[i] = copy[i];

                _previousToSkipLength = _materialsToSkipLength;
            }

            if (_materialsToSkip == null)
                _materialsToSkip = new Object[_materialsToSkipLength];

            EditorGUILayout.LabelField("Materials to Skip", EditorStyles.boldLabel);
            for (int i = 0; i < _materialsToSkipLength; ++i)
                _materialsToSkip[i] = EditorGUILayout.ObjectField(_materialsToSkip[i], typeof(Material), true);

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            if (GUILayout.Button("Randomize selected objects and children", GUILayout.Height(45), GUILayout.ExpandWidth(true)))
            {
                if (_selection.Length == 0)
                {
                    EditorUtility.DisplayDialog("Material randomizer warning", "You must select at least 1 object for randomization !", "OK");
                    return;
                }

                _rndMaterials = new System.Collections.Generic.List<Material>();
                for (int i = _materials.Length - 1; i >= 0; --i)
                    if (_materials[i] is Material mat)
                        _rndMaterials.Add(mat);

                _materialsToSkipList = new System.Collections.Generic.List<Material>();
                for (int i = _materialsToSkip.Length - 1; i >= 0; --i)
                    if (_materialsToSkip[i] is Material mat)
                        _materialsToSkipList.Add(mat);

                if (_rndMaterials.Count == 0)
                {
                    EditorUtility.DisplayDialog("Material randomizer warning", "At least 1 material must be specified !", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(_filter) || string.IsNullOrWhiteSpace(_filter))
                    if (!EditorUtility.DisplayDialog(
                        "Material randomizer warning",
                        "No filter has been specified, ALL child mesh renderers will be affected.",
                        "Continue",
                        "Cancel"))
                        return;

                // Randomization is valid.
                for (int i = _selection.Length - 1; i >= 0; --i)
                    IterateChildren(_selection[i], RandomizeMaterial);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            Repaint();
        }
    }
}