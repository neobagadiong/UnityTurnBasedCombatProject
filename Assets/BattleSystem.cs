using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState { START, PLAYERTURN, ENEMYTURN, WON, LOST, WAITING}
public class BattleSystem : MonoBehaviour
{
	///////////////////////////////////////////////
	//
	// Current bug: -coroutine behaviour causes player to unblock before enemy turn causing player death.
	//
	/////////////////////////////////////////////

	public GameObject playerPrefab;
	public GameObject enemyPrefab;
	public Transform playerBattleStation;
	public Transform enemyBattleStation;
	Character player;
	Character enemy;
	public Text dialogueText;
	
	public CharacterStat playerHUD;
	public CharacterStat enemyHUD;
	public GameState curState;

    // Start is called before the first frame update
    void Start(){
		curState = GameState.START;
		StartCoroutine(SetupBattle());
    }

	IEnumerator SetupBattle(){
		GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
		GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
		player = playerGO.GetComponent<Character>();
		enemy = enemyGO.GetComponent<Character>();
		playerHUD.SetHUD(player);
		enemyHUD.SetHUD(enemy);
		
		dialogueText.text = "You encounter a " + enemy.name + ".";
		yield return new WaitForSeconds(1f);
		dialogueText.text = "Initiating combat protocols...";
		yield return new WaitForSeconds(1f);

		//curState = GameState.PLAYERTURN;	
		StartCoroutine(PlayerTurn());
	}



	void EndBattle(){

		if(curState == GameState.WON)
		{
			dialogueText.text = "YOU WIN";
		} else if (curState == GameState.LOST)
		{
			dialogueText.text = "YOU LOSE.";
		}
	}



	IEnumerator PlayerTurn(){
		if(player.stunned){
			dialogueText.text = "You are stunned.";
			yield return new WaitForSeconds(2f);
			StartCoroutine(EnemyTurn());
		}else {
			
			if(player.blocking){
				player.blocking = false;
				dialogueText.text = "You are no longer on guard.";
				yield return new WaitForSeconds(1f);
			}
			curState = GameState.PLAYERTURN;
			dialogueText.text = "Your turn. What will you do?";


		}


		
	}



	IEnumerator EnemyTurn(){
		bool preventStun = player.stunned;
		player.stunned = false;
		dialogueText.text = enemy.name + "'s turn...";
		bool isDead = false;
		yield return new WaitForSeconds(1f);


		int outgoing = Random.Range(enemy.damage, enemy.damage + 20);
		if(enemy.blocking){
			if(player.blocking){
				 outgoing = player.curHP/10;
			}else{
				outgoing = player.curHP;
			}
			enemy.blocking = false;
			dialogueText.text = enemy.name + " unleashed a powerful attack";
			yield return new WaitForSeconds(1f);
		}else{
			
			//15% chance to charge attack
			if (Random.Range(0,100) >= 85){
				// charge attack for next turn
				enemy.blocking = true;
				dialogueText.text = enemy.name + " analyzes you.";
				outgoing = 0;
				yield return new WaitForSeconds(2f);
				StartCoroutine(PlayerTurn());

			}else {
				//10% chance to crit
				if (Random.Range(0,100) > 89){
					outgoing = (int) (outgoing * 1.6);
					dialogueText.text = "CRITICAL DAMAGE";
					yield return new WaitForSeconds(1f);
					if(preventStun){
						 
						dialogueText.text = "You are already stunned";
						yield return new WaitForSeconds(1f);
					}else{
						dialogueText.text = enemy.name + " stunned you.";
						yield return new WaitForSeconds(1f);
						player.stunned = true;
					}
		

				}

				
				dialogueText.text = enemy.name + " dealt " + outgoing.ToString() + " damage.";
				//yield return new WaitForSeconds(2f);
			}


		}
		isDead = !player.setHP(-outgoing);
		playerHUD.setHP(player.curHP);

		yield return new WaitForSeconds(2f);

		if(isDead)
		{
			curState = GameState.LOST;
			EndBattle();
		} else
		{
			
			StartCoroutine(PlayerTurn());
		}

	}




	IEnumerator PlayerAttack(){
		curState = GameState.WAITING;
		//Random rnd = new Random();
		bool preventStun = enemy.stunned;
		enemy.stunned = false;
		int outgoing = Random.Range(player.damage - 5, player.damage + 15);

		if(Random.Range(0,100) > 84){
			outgoing = (int)(outgoing * 1.6);
			
			dialogueText.text = "CRITICAL DAMAGE.";
			yield return new WaitForSeconds(1f);

			if (preventStun){
				dialogueText.text = "Enemy was already incapacitated.";
				//enemy.stunned = false;
				yield return new WaitForSeconds(1f);
			} else {
				
				enemy.stunned = true;
				
			}
		}

		bool isEnemyDead = !enemy.setHP(-outgoing);

		enemyHUD.setHP(enemy.curHP);
		dialogueText.text = "You dealt " + outgoing.ToString() + " damage.";
		yield return new WaitForSeconds(2f);

		if(isEnemyDead){
			curState = GameState.WON;
			EndBattle();
		}else if (!isEnemyDead & enemy.stunned){
			dialogueText.text = "Critical damage incapacitated enemy.";
			yield return new WaitForSeconds(2f);
			StartCoroutine(PlayerTurn());
		}else{
			curState = GameState.ENEMYTURN;
			StartCoroutine(EnemyTurn());
			
		}
	}

	IEnumerator PlayerGuard(){
		curState = GameState.WAITING;
		dialogueText.text = "You anticipate an upcoming attack.";
		player.blocking = true;

		yield return new WaitForSeconds(2f);
		
		curState = GameState.ENEMYTURN;
		StartCoroutine(EnemyTurn());
	}

	IEnumerator PlayerHeal(){
		curState = GameState.WAITING;
		dialogueText.text = "Health boost consumed.";
		if(player.curHP + 120 >= player.maxHP){
			player.curHP = player.maxHP;
		}else{
			player.curHP = player.curHP + 120;
		}


		playerHUD.setHP(player.curHP);

		yield return new WaitForSeconds(1f);

		curState = GameState.ENEMYTURN;
		StartCoroutine(EnemyTurn());
	}


	IEnumerator PlayerRun(){
		curState = GameState.WAITING;
		dialogueText.text = "Theres no running from this one...";

		yield return new WaitForSeconds(2f);

		StartCoroutine(PlayerTurn());

	}


//"Listeners" for button presses on combat UI


	public void OnAttackButton()
	{
		if (curState != GameState.PLAYERTURN)
			return;

		StartCoroutine(PlayerAttack());
	}

	public void OnGuardButton()
	{
		if (curState != GameState.PLAYERTURN)
			return;

		StartCoroutine(PlayerGuard());
	}

	public void OnHealButton()
	{
		if (curState != GameState.PLAYERTURN)
			return;

		StartCoroutine(PlayerHeal());
	}

	public void OnRunButton()
	{
		if (curState != GameState.PLAYERTURN)
			return;

		StartCoroutine(PlayerRun());
	}


}
