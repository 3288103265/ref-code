#if UNITY_EDITOR
namespace DecalSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Serialization;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class Decal : MonoBehaviour
    {

        [FormerlySerializedAs("material")] public Material Material;
        [FormerlySerializedAs("sprite")] public Sprite Sprite;
        [FormerlySerializedAs("Texture")] private Texture2D Texture;

        [FormerlySerializedAs("affectedLayers"), FormerlySerializedAs("AffectedLayers")] public LayerMask LayerMask = -1;
        [FormerlySerializedAs("maxAngle")] public float MaxAngle = 90.0f;
        [FormerlySerializedAs("pushDistance"), FormerlySerializedAs("PushDistance")] public float Offset = 0.009f;

        private Vector3 oldScale;

        public MeshFilter MeshFilter
        {
            get
            {
                return gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            }
        }
        public MeshRenderer MeshRenderer
        {
            get
            {
                return gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            }
        }


        [MenuItem("GameObject/Decal")]
        internal static void Create()
        {
            new GameObject("Decal", typeof(Decal), typeof(MeshFilter), typeof(MeshRenderer)).isStatic = false;
        }


        public void OnValidate()
        {
            if (!Material) Sprite = null;
            if (Sprite && Material.mainTexture != Sprite.texture) Sprite = null;

            MaxAngle = Mathf.Clamp(MaxAngle, 1, 180);
            Offset = Mathf.Clamp(Offset, 0.005f, 0.05f);
        }


        void OnEnable()
        {
            if (Application.isPlaying) enabled = false;
        }

        private void InitDecal()
        {
            Texture = new Texture2D(50, 50, TextureFormat.ARGB32, false);
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    Texture.SetPixel(i, j, new Color(i / 50f, j / 50f, 0, 1));
                }
            }
            Texture.Apply();

            Material.SetTexture("_MainTex", Texture);
            Sprite = Sprite.Create(Texture, new Rect(0.0f, 0.0f, Texture.width, Texture.height), new Vector2(0.5f, 0.5f), 1f);
            Debug.Log($"Initlized decal.");
           
        }
        private byte[] recvBytes;

        void Start()
        {
            recvBytes = new byte[65535];
            InitDecal();

        }

        void Update()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                BuildAndSetDirty();
            }

        }


        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            var bounds = DecalUtils.GetBounds(this);
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size + Vector3.one * 0.01f);
        }


        public void BuildAndSetDirty()
        {
            if (Sprite) DecalUtils.FixRatio(this, ref oldScale);
            DecalBuilder.Build(this);
            DecalUtils.SetDirty(this);
        }

    }
}
#endif