using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStat : MonoBehaviour
{   
	// create 
	public Text name;
	public Slider hpBar;
	
	public Text curHPtxt;

	public void SetHUD(Character chara)
	{
		
		curHPtxt.text = chara.curHP.ToString();
		name.text = chara.name;
		hpBar.maxValue = chara.maxHP;
		hpBar.value = chara.curHP;
	}


	public void setHP(int hp)
	{	
		curHPtxt.text = hp.ToString();
		hpBar.value = hp;
	}

}
