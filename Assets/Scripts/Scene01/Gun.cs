using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireInterval = 0.5f;
    public Transform muzzleTransform;
    public float bulletVelocity = 5f;

    private float _nextFire = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // 0 表示左键；1 表示右键；2 表示中键
        if (Input.GetMouseButton(0) && Time.time > _nextFire)
        {
            _nextFire = Time.time + fireInterval;
            GameObject bullet = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation);
            bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.right * bulletVelocity;
        }
    }
}