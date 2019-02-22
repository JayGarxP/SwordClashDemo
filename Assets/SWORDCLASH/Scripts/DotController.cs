using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRubyShared;

namespace SwordClash
{
    public class DotController : MonoBehaviour
    {

        public SpriteRenderer m_SpriteRenderer;

        // Use this for initialization
        void Start()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();

        }

        //// Update is called once per frame
        //void Update()
        //{

        //}

        public void OnSwipe(SwipeGestureRecognizerDirection whichSwipe)
        {
            switch (whichSwipe)
            {
                case SwipeGestureRecognizerDirection.Up:
                    {
                        OnUpSwiped();
                        break;
                    }
                case SwipeGestureRecognizerDirection.Down:
                    {
                        OnDownSwiped();
                        break;
                    }
                case SwipeGestureRecognizerDirection.Left:
                    {
                        OnLeftSwiped();
                        break;
                    }
                case SwipeGestureRecognizerDirection.Right:
                    {
                        OnRightSwiped();
                        break;
                    }
                default:
                    //Console.WriteLine("Default case");
                    break;
            }
        }

        public void OnUpSwiped()
        {
            m_SpriteRenderer.color = Color.blue;
        }
        public void OnDownSwiped()
        {
            m_SpriteRenderer.color = Color.magenta;
        }
        public void OnLeftSwiped()
        {
            m_SpriteRenderer.color = Color.black;
        }
        public void OnRightSwiped()
        {
            m_SpriteRenderer.color = Color.green;
        }


    }
}