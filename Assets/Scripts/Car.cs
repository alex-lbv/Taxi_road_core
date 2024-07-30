﻿using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Car : MonoBehaviour
{
    private enum State
    {
        MoveForward,
        MoveBackward,
        Idle
    }


    [SerializeField]
    private CarRuleSet carRuleSet;

    public event EventHandler onClickHandler;
    public bool isMove = false;
    public bool isMoveBackward = false;
    private int currentRule = 0;

    private Transform currentTarget;
    private Guid carId;

    public List<Vector3> waypoints;

    private State state;

    private void Start()
    {
        carId = Guid.NewGuid();

        waypoints = new List<Vector3>();

        SetStartPostionToWaypoints();

        state = State.Idle;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleIntersection(other);
        HandleCarCollision(other);
    }

    private void HandleIntersection(Collider other)
    {
        Intersection intersection = other.GetComponent<Intersection>();
        if (intersection != null && isMove)
        {
            currentTarget = intersection.GetClosestWaypoint(transform.position);
        }
    }

    private void HandleCarCollision(Collider other)
    {
        Car otherCar = other.GetComponent<Car>();

        Debug.Log("Collide");

        if (otherCar != null && !isMoveBackward)
        {
         
            //Debug.Log(carId.CompareTo(otherCar.carId));

            if (isMove)
            {
                isMove = false;
                isMoveBackward = true;
                /*currentTarget = waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : null; */
            }
            return;
        }
    }

    private void CheckWaypoint()
    {
        if (currentTarget != null && !isMoveBackward)
        {
            Vector3 currentPosition = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 targetPosition = new Vector3(currentTarget.position.x, 0, currentTarget.position.z);
            float distance = Vector3.Distance(currentPosition, targetPosition);
            if (distance <= 0.1f)
            {
                transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
                ExecuteCurrentRule();
                currentTarget = null;
                /*                if (isMoveBackward)
                                {
                                    ExecuteCurrentRuleBackward();
                                    waypoints.RemoveAt(waypoints.Count - 1); // Удаляем достигнутую точку
                                    if (waypoints.Count == 0)
                                    {
                                        isMoveBackward = false;
                                        isMove = false;
                                        Debug.Log("Reached the start position. Stopping the car.");
                                    }
                                    else
                                    {
                                        currentTarget = waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : null; // Устанавливаем следующую точку как цель
                                    }
                                }
                                else
                                {
                                    currentTarget = null;
                                    ExecuteCurrentRule();
                                }*/
            }
        }
    }

    private void ExecuteCurrentRule()
    {
        if (currentRule < carRuleSet.rules.Length)
        {
            waypoints.Add(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));
            carRuleSet.rules[currentRule].Execute(this);
            currentRule++;
        }
    }

    private void ExecuteCurrentRuleBackward()
    {
        if (currentRule > 0)
        {
            currentRule--;
            carRuleSet.rules[currentRule].Execute(this);
        }
    }

    public void MoveForward()
    {
        isMove = true;
    }

    public void TurnLeft()
    {
        if (isMoveBackward)
        {
            transform.Rotate(0, 90, 0);
        }
        else
        {
            transform.Rotate(0, -90, 0);
            isMove = true;
        }
    }

    public void TurnRight()
    {
        if (isMoveBackward)
        {
            transform.Rotate(0, -90, 0);
        }
        else
        {
            transform.Rotate(0, 90, 0);
            isMove = true;
        }
    }

    private void Move()
    {
        Vector3 newPos = 2 * Time.deltaTime * transform.forward;
        transform.position = transform.position + newPos;
    }

    private void MoveBackward()
    {
        if (waypoints.Count == 0)
            {
                isMoveBackward = false;
                SetStartPostionToWaypoints();
                return;
            }

        /*       if(waypoints.Count == 0)
               {
                   Debug.Log("no points");
                   isMoveBackward = true;
                   return;
               }*/

        Vector3 targetPoint = waypoints[waypoints.Count -1];
        Vector3 target = new Vector3(targetPoint.x, targetPoint.y, targetPoint.z);
        // Игнорируем ось Y
        target.y = transform.position.y;

        Vector3 dir = target - transform.position;
        Vector3 newPos = 2 * Time.deltaTime * dir.normalized;

        transform.position = transform.position + newPos;
        float dist = Vector3.Distance(target, transform.position);

        if (dist < .01f)
        {
            transform.position = target;
            waypoints.RemoveAt(waypoints.Count - 1);
            ExecuteCurrentRuleBackward();

    /*        if (waypoints.Count == 0)
            {
                isMoveBackward = false;
                SetStartPostionToWaypoints();
                return;
            }*/
        }
    }

    public void SetStartPostionToWaypoints()
    {
        
        waypoints.Add(new Vector3(transform.position.x, transform.position.y, transform.position.z));
    }


    public Sprite[] GetRuleSprites()
    {
        return carRuleSet.sprites;
    }

    void Update()
    {
        if (isMove)
        {
            CheckWaypoint();
            Move();
            return;
        }

        if (isMoveBackward)
        {
            /*CheckWaypoint();*/
            MoveBackward();
        }
    }
}