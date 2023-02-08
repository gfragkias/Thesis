using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [Tooltip("The particle when the fire is healthy")]
    public ParticleSystem fireParticle;

    [Tooltip("The particle when the fire is extinguished")]
    public ParticleSystem smokeParticle;

    /// <summary>
    /// The trigger collider representing the place
    /// of the fire you have interact to extinguish it.
    /// </summary>
    [HideInInspector]
    public Collider fireCollider;

    /// <summary>
    /// The solid collider representing the physical 
    /// collision of the fire object.
    /// </summary>
    [HideInInspector]
    public Collider solidFireCollider;


    /// <summary>
    /// A vector pointing straight from the fire
    /// </summary>
    public Vector3 FireUpVector
    {
        get
        {
            return fireCollider.transform.forward;
        }
    }

    /// <summary>
    /// The center position of the fire collider
    /// </summary>
    public Vector3 FireCenterPosition
    {
        get
        {
            return fireCollider.transform.position;
        }
    }


    /// <summary>
    /// The remaining 'health' of the fire
    /// </summary>
    public float FireAmount { get; private set; }

    /// <summary>
    /// Whether the fire has any health left
    /// </summary>
    public bool OnFire
    {
        get
        {
            return FireAmount > 0f;
        }
    }

    /// <summary>
    /// Attempts to extinguish remaining fire
    /// </summary>
    /// <param name="amount"> The amount of "health" remaining to the fire</param>
    /// <returns> The actual amount of fire succesfuly put off</returns>
    public float Extinguish(float amount)
    {
        //Track how much fire was succesfully put off(you can't take more than the available amount)
        float healthExtinguished = Mathf.Clamp(amount, 0f, FireAmount);

        //Substract the fire's health
        FireAmount -= amount;

        if (FireAmount <= 0)
        {
            // I have to be very careful so I set remaining fire to 0
            FireAmount = 0;

            //Disable the fire collider
            fireCollider.gameObject.SetActive(false);
            solidFireCollider.gameObject.SetActive(false);

            //Change the object particle to smoke
            fireParticle.Stop();
            smokeParticle.Play();
        }

        //Return the amount of health remaining
        return healthExtinguished; 
    }


    public void ResetFire()
    {
        //Restart the fire
        FireAmount = 1f;

        //Enable the fire collider
        fireCollider.gameObject.SetActive(true);
        solidFireCollider.gameObject.SetActive(true);

        //Change the object to indicate that it's on fire
        fireParticle.Play();
        smokeParticle.Stop();
    }

    /// <summary>
    /// Called when fire "wakes" up
    /// </summary>
    private void Awake()
    {
        //Find fire collider
        fireCollider = transform.Find("FireCollider").GetComponent<Collider>();
        solidFireCollider = transform.Find("SolidFireCollider").GetComponent<Collider>();



        //Find the particle system to get the fire particles
        fireParticle = transform.Find("FireParticle").GetComponent<ParticleSystem>();
        smokeParticle = transform.Find("SmokeParticle").GetComponent<ParticleSystem>();
        fireParticle.Play();
        smokeParticle.Stop();
    }

}
