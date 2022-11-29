using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character: MonoBehaviour
{

	public string name;
	public int damage;
	public int maxHP;
	public int curHP;
	public bool stunned;
	public bool blocking;

	public bool setHP(int hpDiff)
	{
		curHP += hpDiff;
		if (curHP <= 0)
			return false;
		else
			return true;
	}
 
}
