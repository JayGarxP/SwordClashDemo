using UnityEngine;

namespace SwordClash
{
    class SwordClashFoodController : Bolt.EntityEventListener<ITentacleFoodState>
    {
        [SerializeField]
        private Sprite[] SnackSpriteArray;
      
        private SpriteRenderer SnackSprite;
        private Rigidbody2D SnackBody;

        // Bolt network UnityEngine.Start()
        public override void Attached()
        {
            if (entity.isOwner)
            {
                state.SetTransforms(state.FoodTransform, transform);
            }
        }

        private void Update()
        {
            SnackSprite.sprite = SnackSpriteArray[state.SpriteIndex];
        }

        //public override void SimulateOwner()
        //{
        //    // Attempt to keep sprite in sync over Bolt network.
        //    SnackSprite.sprite = SnackSpriteArray[state.SpriteIndex];
        //}

        //public override void SimulateController()
        //{
        //    // Attempt to keep sprite in sync over Bolt network.
        //    SnackSprite.sprite = SnackSpriteArray[state.SpriteIndex];
        //}

        // Test to see if OnEnable works with Instantiate. May have to use constructor
        void OnEnable()
        {
            SnackBody = GetComponent<Rigidbody2D>();
            SnackSprite = GetComponent<SpriteRenderer>();
        }

        // Get a new random SnackSprite and then move the snack to the center of screen
        public int MoveFoodToCenter(Vector3 ScreenCenterWorldUnits)
        {
            // The index is saved so the correct sprite can be synced across the Bolt network.
             int spriteIndex =
             AssignRandomSnackSprite();

            SnackBody.position = (ScreenCenterWorldUnits);

            return spriteIndex;
        }

        // Returns random index after assigning Snack Sprite
        private int AssignRandomSnackSprite()
        {
            // Assuming that the SnackSpriteArray is of size 20, with 20 food sprites set in the Unity Editor Inspector
            //  Pick a random sprite to be the current SnackSprite;
            // UnityEngine.Random.Range is min inclusive; max exlcusive; so it is safe to use length of array as max.
            int randomIndexIntoSnackSpriteArray = UnityEngine.Random.Range(0, SnackSpriteArray.Length);
            SnackSprite.sprite = SnackSpriteArray[randomIndexIntoSnackSpriteArray];
            state.SpriteIndex = randomIndexIntoSnackSpriteArray;
            return randomIndexIntoSnackSpriteArray;
        }

    }// end class definition
}// end namespace
