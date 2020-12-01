using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class CardParser
{
    /// <summary>
    /// Returns card struct from string value
    /// </summary>
    public static Card parseCard(string cardName)
    {
        string[] values = cardName.Split('_');

        Suit newSuit;
        Enum.TryParse(values[0], out newSuit);
        int newNumber;
        int.TryParse(values[1], out newNumber);

        Card newCard = new Card();
        newCard.suit = newSuit;
        newCard.number = newNumber;

        return newCard;
    }

    /// <summary>
    /// Returns card struct to its string value
    /// </summary>
    public static string deparseCard(Card card)
    {
        return card.suit.ToString() + "_" + card.number;
    }
}

[System.Serializable]
public struct Card
{
    public Suit suit;
    public int number;
}

public enum Suit
{
    Club,
    Spade,
    Diamond,
    Heart,
    Joker
}