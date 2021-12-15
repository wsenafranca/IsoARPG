using UnityEngine;

namespace Utils
{
    [System.Serializable]
    public class SingleUnityLayer
    {
        [SerializeField]
        private int layerIndex = 0;
    
        public int index
        {
            get => layerIndex;
            set
            {
                if (value is > 0 and < 32)
                {
                    layerIndex = value;
                }
            }
        }
  
        public int mask => 1 << layerIndex;
    }
}
