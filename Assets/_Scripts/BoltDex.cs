using System.Collections;
using System.Collections.Generic;
using Thirdweb;
using UnityEngine;



public class BoltDex : MonoBehaviour
{
    private const string DEX_CONTRACT = "0xF5A5B8989Ac6182bE47C58f08dD15Ee1Ce1a0AC5";
    private const string WBTC_CONTRACT = "0x9F5E23678958D139Cacf2b2a0e6Cc80e11EEbA60";
    private const string WBNB_CONTRACT = "0x5Ebc47F184078773f982F12F5d47CD0d96c2c085";
    private const string WETH_CONTRACT = "0xf3D79beb24Eabf1BA3D0DdA585e3D323D7A8A142";
    private const string WPLS_CONTRACT = "0x3d530598DA65d7c7907f3f99A8Dd2915bD55Ace8";
    private const string LINK_CONTRACT = "0xac69BecD5Df35034Be894df5fc379C125DC19415";

    public async void GetBalance()
    {
        try
        {
            CurrencyValue balance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            Debugger.Instance.Log("[Get Balance] Native Balance", balance.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Get Balance] Error", e.Message);
        }
    }

    private string GetContractAbi()
    {
        // Here goes the Exchange 
        return @"[
            {
                ""inputs"": [],
                ""stateMutability"": ""nonpayable"",
                ""type"": ""constructor""
            },
            {
                ""inputs"": [
                    {
                        ""internalType"": ""string"",
                        ""name"": ""tokenName"",
                        ""type"": ""string""
                    },
                    {
                        ""internalType"": ""address"",
                        ""name"": ""_address"",
                        ""type"": ""address""
                    }
                ],
                ""name"": ""getBalance"",
                ""outputs"": [
                    {
                        ""internalType"": ""uint256"",
                        ""name"": """",
                        ""type"": ""uint256""
                    }
                ],
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""internalType"": ""string"",
                        ""name"": ""tokenName"",
                        ""type"": ""string""
                    }
                ],
                ""name"": ""getTokenAddress"",
                ""outputs"": [
                    {
                        ""internalType"": ""address"",
                        ""name"": """",
                        ""type"": ""address""
                    }
                ],
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""internalType"": ""string"",
                        ""name"": ""tokenName"",
                        ""type"": ""string""
                    }
                ],
                ""name"": ""swapETHtoToken"",
                ""outputs"": [
                    {
                        ""internalType"": ""uint256"",
                        ""name"": """",
                        ""type"": ""uint256""
                    }
                ],
                ""stateMutability"": ""payable"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""internalType"": ""string"",
                        ""name"": ""srcTokenName"",
                        ""type"": ""string""
                    },
                    {
                        ""internalType"": ""string"",
                        ""name"": ""destTokenName"",
                        ""type"": ""string""
                    },
                    {
                        ""internalType"": ""uint256"",
                        ""name"": ""_amount"",
                        ""type"": ""uint256""
                    }
                ],
                ""name"": ""swapTokenToToken"",
                ""outputs"": [],
                ""stateMutability"": ""nonpayable"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""internalType"": ""string"",
                        ""name"": ""tokenName"",
                        ""type"": ""string""
                    },
                    {
                        ""internalType"": ""uint256"",
                        ""name"": ""_amount"",
                        ""type"": ""uint256""
                    }
                ],
                ""name"": ""swapTokentoETH"",
                ""outputs"": [
                    {
                        ""internalType"": ""uint256"",
                        ""name"": """",
                        ""type"": ""uint256""
                    }
                ],
                ""stateMutability"": ""nonpayable"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""internalType"": ""string"",
                        ""name"": """",
                        ""type"": ""string""
                    }
                ],
                ""name"": ""tokenInstanceMap"",
                ""outputs"": [
                    {
                        ""internalType"": ""contract ERC20"",
                        ""name"": """",
                        ""type"": ""address""
                    }
                ],
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""internalType"": ""uint256"",
                        ""name"": """",
                        ""type"": ""uint256""
                    }
                ],
                ""name"": ""tokens"",
                ""outputs"": [
                    {
                        ""internalType"": ""string"",
                        ""name"": """",
                        ""type"": ""string""
                    }
                ],
                ""stateMutability"": ""view"",
                ""type"": ""function""
            }
        ]";
    }
}
