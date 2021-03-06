﻿using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusWindow : MonoBehaviour
{
    private Image image;
    private Text message;

    // Start is called before the first frame update
    void Awake()
    {
        this.image = this.GetComponent<Image>();
        this.message = GameObject
            .Find("PlayerStatusWindowText")
            .GetComponent<Text>();
    }

    public void Open()
    {
        this.image.enabled = true;
    }

    public void Close()
    {
        this.image.enabled = false;
        this.message.text = "";
    }

    public void SetPlayer(Player player)
    {
        this.message.text = $"{player.name}\nAge: {player.Age}\nParent Age: {player.ParentAge}\nQoL: {player.QoL}\nMoney: {player.Money}\nTime: {player.Time}";
    }
}
