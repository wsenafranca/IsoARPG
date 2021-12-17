using Item;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class CommandManager : MonoBehaviour
    {
        public ItemBase[] genItems;

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Q) || genItems.Length <= 0) return;
            
            
            var item = genItems[Random.Range(0, genItems.Length)];
            var itemDrop = Instantiate(item.itemSlotPrefab);
            InputController.instance.GetGroundPosition(Input.mousePosition, out var pos);
            itemDrop.GetComponent<Collectible>().SetAsDrop(item, null, pos);
        }
    }
}