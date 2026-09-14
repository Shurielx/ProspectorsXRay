using System;
using System.Collections.Generic;
using HarmonyLib;
using ProspectorsXRay.Network;
using ProspectorsXRay.OreDatabase.Models;
using ProspectorsXRay.OreDatabase.Services;
using ProspectorsXRay.Survey;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ProspectorsXRay;

public class ProspectorsXRayModSystem : ModSystem
{
    private const string HarmonyId = "com.shuriel.prospectorsxray";
    private const string NetworkChannelName = "prospectorsxray";

    private Harmony? harmony;
    private Dictionary<int, OreInfo> oreDatabase = new();

    public static ICoreClientAPI? ClientApi { get; private set; }
    public static SurveyEngine? ServerSurveyEngine { get; private set; }
    public static ActiveSurveyClient? ClientSurvey { get; private set; }

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);

        try
        {
            harmony = new Harmony(HarmonyId);
            harmony.PatchAll();
            Patches.ItemProspectingPickPatch.PatchAllProspectingPicks(harmony, api);
            api.Logger.Notification("[Prospector's X-Ray] Harmony patches applied successfully.");
        }
        catch (Exception ex)
        {
            api.Logger.Error($"[Prospector's X-Ray] Failed to apply Harmony patches: {ex}");
        }
    }

    public override void StartServerSide(ICoreServerAPI sapi)
    {
        base.StartServerSide(sapi);

        IServerNetworkChannel serverChannel = sapi.Network.RegisterChannel(NetworkChannelName)
            .RegisterMessageType<SurveyResultPacket>();

        ServerSurveyEngine = new SurveyEngine(sapi, serverChannel, () => oreDatabase);
        sapi.Logger.Notification("[Prospector's X-Ray] Server survey engine initialized.");

        sapi.ChatCommands.Create("ore")
            .WithDescription("Prospector's X-Ray command")
            .HandleWith(_ => TextCommandResult.Success());
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        ClientApi = capi;

        IClientNetworkChannel clientChannel = capi.Network.RegisterChannel(NetworkChannelName)
            .RegisterMessageType<SurveyResultPacket>();

        ClientSurvey = new ActiveSurveyClient(capi);
        clientChannel.SetMessageHandler<SurveyResultPacket>(packet =>
        {
            ClientSurvey.OnSurveyResultReceived(packet);
        });

        capi.Logger.Notification("[Prospector's X-Ray] Client survey renderer initialized.");
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);

        try
        {
            OreDatabaseBuilder builder = new OreDatabaseBuilder(api);
            oreDatabase = builder.Build();

            if (harmony != null)
            {
                Patches.ItemProspectingPickPatch.PatchAllProspectingPicks(harmony, api);
            }
        }
        catch (Exception ex)
        {
            api.Logger.Error($"[Prospector's X-Ray] Failed in AssetsFinalize: {ex}");
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        try
        {
            harmony?.UnpatchAll(HarmonyId);
        }
        catch { }

        ClientSurvey?.Dispose();
        ClientSurvey = null;
        ServerSurveyEngine = null;
        ClientApi = null;
    }
}
