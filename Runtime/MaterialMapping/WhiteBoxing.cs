//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace UnityEngine.Reflect.Extensions.MaterialMapping
//{
//    public class WhiteBoxing : MonoBehaviour
//    {
//        [SerializeField] Material opaque = default;
//        [SerializeField] Material transparent = default;
//        [SerializeField] Material cutout = default;

//        SyncManager syncManager;

//        private void Awake()
//        {
//            syncManager = FindObjectOfType<SyncManager>();

//            if (syncManager == null)
//            {
//                enabled = false;
//                return;
//            }

//            syncManager.onInstanceAdded += InstanceAdded;
//        }

//        private void InstanceAdded(SyncInstance instance)
//        {
//            instance.onObjectCreated += ObjectCreated;
//        }

//        private void ObjectCreated(SyncObjectBinding obj)
//        {
//            Renderer r = obj.GetComponent<Renderer>();
//            if (r = null)
//                return;

//            if (r.sharedMaterials.Length == 1)
//            {

//            }
//            else
//            {

//            }
//        }

//        private void ReplaceMaterial(Material material)
//        {
            
//        }
//    }
//}