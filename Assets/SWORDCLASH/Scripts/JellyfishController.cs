using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SwordClash
{
    // Should each jellyfish have controller or should one controller manage each jellyfish in scene???
    public class JellyfishController : MonoBehaviour
    {
        //// OnCollisionEnter() is event Publisher for trigger collision (hit jellyfish); Subscriber to collision event is tentacleController (where state machine will be implemented)
        //public event EventHandler JellyfishHitByTentacleTip_Event;

        public enum OccilationFuntion { Sine, Cosine }

        private Vector2 startingPosition;


        // Use this for initialization
        void Start()
        {
            startingPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            // If jellyfish has a rigidbody, then oscillate back and forth real smooth-like
            if (GetComponent<Rigidbody2D>() != null)
            {
                Oscillate(OccilationFuntion.Sine, 1.5f);
            }
        }

        // Dis dat Clinton Rodgers jam right here.
        // to start at scalar value
        private void Oscillate(OccilationFuntion method, float scalar)
        {
                if (method == OccilationFuntion.Sine)
                {
                GetComponent<Rigidbody2D>().MovePosition(new Vector2(startingPosition.x + Mathf.Sin(Time.time) * scalar, startingPosition.y));
                   //transform.position = new Vector3(Mathf.Sin(Time.time) * scalar, transform.position.y, transform.position.z);
                }
                else if (method == OccilationFuntion.Cosine)
                {
                    transform.position = new Vector3(Mathf.Cos(Time.time) * scalar, 0, 0);
                }
            
        }
    
        // Pretty sure only the tentacle needs MB.OnTriggerEnter2D(); simply make jellyfish triggers

        //private void OnTriggerEnter2D(Collider2D collision)
        //{
        //    ////Raise collide event, subscribed to in TentacleController
        //    //JellyfishHitByTentacleTip_Event(this, EventArgs.Empty);

        //    ////the subscriber class needs a reference to the publisher class in order to subscribe to its events.
        //    //void HandleCustomEvent(object sender, CustomEventArgs a)
        //    //{
        //    //    // Do something useful here.  
        //    //}

        //    //Subscripe +=; Unsub -=;
        //    //publisher.RaiseCustomEvent += HandleCustomEvent;  
        //    //  publisher.RaiseCustomEvent -= HandleCustomEvent; 

        //}

    }
}
