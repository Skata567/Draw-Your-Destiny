using NYH.CoreCardSystem;
using UnityEngine;

public class CountCardByTypeGA : GameAction
{
	CardType _CardType;

	public CountCardByTypeGA(CardType cardType)
	{
		_CardType = cardType;
	}
}
