﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CreatureMovement : MonoBehaviour
{

    public Transform creatureTransform;
    public CreatureAttributes attributes;

    private float velocityConsumFactor = 0.5f;
    private float nextSpawn = 0;
    private Collider2D targetFood;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Move() {
        Vector3 moveVector;

        switch (attributes.State) {
            case CreatureAttributes.CreatureState.Wander:
                moveVector = CreateRandomVector();
                break;
            case CreatureAttributes.CreatureState.Hunt:
                moveVector = CreateHuntVector(attributes.TargetFood);
                break;
            default:
                moveVector = CreateRandomVector();
                break;
        }

        creatureTransform.position += moveVector * attributes.Velocity * Time.deltaTime;

        // energy consumed at rate of (velocityConsumFactor * velocity^2) units per second
        attributes.Energy -= velocityConsumFactor * Mathf.Pow(attributes.Velocity, 2) * Time.deltaTime;

        // Ensure creature remains within bounds
        if (creatureTransform.position.x > attributes.Bounds.x || creatureTransform.position.x < -attributes.Bounds.x ||
            creatureTransform.position.y > attributes.Bounds.y || creatureTransform.position.y < -attributes.Bounds.y) {
            creatureTransform.position -= moveVector;
        }
    }

    private Vector3 CreateRandomVector() {
        return Random.insideUnitCircle.normalized;
    }

    private Vector3 CreateHuntVector(Collider2D prey) {
        return (prey.transform.position - creatureTransform.position).normalized;
    }

    private void Reproduce() {
        if (Time.time > nextSpawn) {
            nextSpawn = Time.time + attributes.SpawnRate;

            if ( (attributes.Energy > 2 * attributes.StartingEnergy) && (Random.Range(0.0f, 1.0f) > attributes.SpawnChance) ) {
                Vector3 offsetVector = new Vector3(creatureTransform.localScale.x, creatureTransform.localScale.y);
                Instantiate(gameObject, creatureTransform.position + offsetVector, Quaternion.identity);

                attributes.Energy -= attributes.StartingEnergy;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        Move();
        Reproduce();
    }

    // Collision
    void OnCollisionEnter2D(Collision2D collision) {
        // Eat food
        if (collision.gameObject.tag.Equals("Food")) {
            attributes.Energy += collision.gameObject.GetComponent<Nutrition>().NutritionalValue;
            Destroy(collision.gameObject);
        }
    }
}