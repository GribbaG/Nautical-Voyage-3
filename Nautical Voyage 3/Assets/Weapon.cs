using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
	public class Weapon : MonoBehaviour
	{

		public Transform firePoint;
		public GameObject harpoonPrefab;


		// Update is called once per frame
		void Update()
		{
			if (Input.GetButtonDown("Fire1"))
			{
				Shoot();
			}
		}

		void Shoot()
		{
			//shooting logic
			Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation);
		}

	}
}