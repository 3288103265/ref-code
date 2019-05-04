using UnityEngine;

namespace _Decal {

    [RequireComponent( typeof( MeshFilter ) )]
    [RequireComponent( typeof( MeshRenderer ) )]
    [ExecuteInEditMode]
    public class Decal : MonoBehaviour {

        public Material material;
        public Sprite sprite;
        private Texture2D tex;
        private uint count;

        public float maxAngle = 90.0f;
        public float pushDistance = 0.009f;
        public LayerMask affectedLayers = -1;

        public Texture texture {
            get {
                return material ? material.mainTexture : null;
            }
        }

        void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube( Vector3.zero, Vector3.one );
        }

        void Start()
        {
            count = 0;
            InitDecal();
        }

        void Update()
        {
            ChangeImg();
        }

        private void ChangeImg()
        {
            count++;
            if (count <156)
            {
                tex = Resources.Load<Texture2D>(count.ToString());
                material.SetTexture("_MainTex", tex);
                sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            }
            else
            {
                return;
            }
        }

        private void InitDecal()
        {
            material = Resources.Load<Material>("Decal_1");
            tex = Resources.Load<Texture2D>("cartoon");
            material.SetTexture("_MainTex", tex);
            sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            Debug.Log("Initlized decal.");
        }
    }
}
