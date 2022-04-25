using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Equipment
{
    public class WeaponWheelButtonController : EventTrigger
    {
        [SerializeField]
        private ItemType ID;

        private Animator anim;
        public Image selectedItem;
        public Sprite icon;
        private bool selected;

        private bool isDragged;

        private const string SELECT_CONDITION = "Select";

        public delegate void SelectItem(ItemType ID);
        public event SelectItem SelectItemEvent;

        public delegate void SpinWheel(float deltaX);
        public event SpinWheel SpinWheelEvent;

        public delegate void UseWeapon(string wormId);
        public event UseWeapon UseWeaponEvent;

        private Vector3 lastDragPosition;

        private void Awake()
        {
            isDragged = false;
            selected = false;
        }   

        // Start is called before the first frame update
        void Start()
        {
            anim = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            SpinWheelEvent += FindObjectOfType<WeaponWheelController>().onSpinWheel;
            SelectItemEvent += FindObjectOfType<WeaponWheelController>().onSelectItem;
            UseWeaponEvent += FindObjectOfType<Worm.WormManager>().onUseItem;
        }

        public override void OnPointerUp(PointerEventData data)
        {
            if (!isDragged)
            {
                // if weapon is not selected already, select it
                if (!selected)
                {
                    Selected();
                }
                else // otherwise fire the selected weapon
                {
                    UseWeaponEvent(Map.HumanSpawnGenerator.HUMAN_WORM_ID);
                }
            }
            isDragged = false;
        }

        public override void OnBeginDrag(PointerEventData data)
        {
            isDragged = true;
            lastDragPosition = data.position;
            print("drag start");
        }

        public override void OnDrag(PointerEventData data)
        {
            float deltaX = data.position.x - lastDragPosition.x;
            lastDragPosition = data.position;
            SpinWheelEvent(deltaX);
        }

        public void Selected() // if item is not already selected equip
        {
            selected = true;
            anim.SetBool(SELECT_CONDITION, true);
            SelectItemEvent(ID);
        }

        public void onDeselected(ItemType selectedID)
        {
            if (selected && ID != selectedID)
            {
                selected = false;
                anim.SetBool(SELECT_CONDITION, false);
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<WeaponWheelController>())
            {
                SpinWheelEvent -= FindObjectOfType<WeaponWheelController>().onSpinWheel;
                SelectItemEvent -= FindObjectOfType<WeaponWheelController>().onSelectItem;
            }
            if (FindObjectOfType<Worm.WormManager>())
            {
                UseWeaponEvent -= FindObjectOfType<Worm.WormManager>().onUseItem;
            }
        }
    }

}
