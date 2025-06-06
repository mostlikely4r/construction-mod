//$MissionName is the file name of the mission
//$MapName is the displayed name(no underscore,spaces)
//$GameType (CTF,Hunters)


function DefaultGame::activatePackages(%game)
{
   // activate the default package for the game type
   activatePackage(DefaultGame);
   if(isPackage(%game.class) && %game.class !$= DefaultGame)
      activatePackage(%game.class);
}

function DefaultGame::deactivatePackages(%game)
{
   deactivatePackage(DefaultGame);
   if(isPackage(%game.class) && %game.class !$= DefaultGame)
      deactivatePackage(%game.class);
}

package DefaultGame {

function FlipFlop::objectiveInit(%data, %flipflop)
{
   // add this flipflop to missioncleanup
   %flipflopSet = nameToID("MissionCleanup/FlipFlops");
   if(%flipflopSet <= 0) {
      %flipflopSet = new SimSet("FlipFlops");
      MissionCleanup.add(%flipflopSet);
   }
   %flipflopSet.add(%flipflop);

   // see if there's a holo projector associated with this flipflop
   // search the flipflop's folder for a holo projector
   // if one exists, associate it with the flipflop

   %flipflop.projector = 0;
   %folder = %flipflop.getGroup();
   for(%i = 0; %i < %folder.getCount(); %i++)
   {
      %proj = %folder.getObject(%i);
      // weird, but line below prevents console error
      if(%proj.getClassName() !$= "SimGroup" && %proj.getClassName() !$= "InteriorInstance")
	 if(%proj.getDatablock().getName() $= "LogoProjector")
	 {
	    %flipflop.projector = %proj;
	    %flipflop.projector.holo = 0;
	    break;
	 }
   }

   // may have been hidden
   %target = %flipFlop.getTarget();
   if(%target != -1)
   {
      // set flipflop to base skin
      setTargetSkin(%target, $teamSkin[0]);

      // make this always visible in the commander map
      setTargetAlwaysVisMask(%target, 0xffffffff);

      // make this always visible in the commander list
      setTargetRenderMask(%target, getTargetRenderMask(%target) | $TargetInfo::CommanderListRender);
   }
}

function FlipFlop::playerTouch(%data, %flipflop, %player)
{
   %client = %player.client;
   %flipTeam = %flipflop.team;

   if(%flipTeam == %client.team)
      return false;

   %teamName = game.getTeamName(%client.team);
   // Let the observers know:
   messageTeam( 0, 'MsgClaimFlipFlop', '\c2%1 claimed %2 for %3.~wfx/misc/flipflop_taken.wav', %client.name, Game.cleanWord( %flipflop.name ), %teamName );
   // Let the teammates know:
   messageTeam( %client.team, 'MsgClaimFlipFlop', '\c2%1 claimed %2 for %3.~wfx/misc/flipflop_taken.wav', %client.name, Game.cleanWord( %flipflop.name ), %teamName );
   // Let the other team know:
   %losers = %client.team == 1 ? 2 : 1;
   messageTeam( %losers, 'MsgClaimFlipFlop', '\c2%1 claimed %2 for %3.~wfx/misc/flipflop_lost.wav', %client.name, Game.cleanWord( %flipflop.name ), %teamName );

   logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") claimed flipflop "@%flipflop@" for team "@%client.team);

   //change the skin on the switch to claiming team's logo
   setTargetSkin(%flipflop.getTarget(), game.getTeamSkin(%player.team));
   setTargetSensorGroup(%flipflop.getTarget(), %player.team);

   // if there is a "projector" associated with this flipflop, put the claiming team's logo there
   if(%flipflop.projector > 0)
   {
      %projector = %flipflop.projector;
      // axe the old projected holo, if one exists
      if(%projector.holo > 0)
	 %projector.holo.delete();

      %newHolo = getTaggedString(game.getTeamSkin(%client.team)) @ "Logo";

      %projTransform = %projector.getTransform();
      // below two functions are from deployables.cs
      %projRot = rotFromTransform(%projTransform);
      %projPos = posFromTransform(%projTransform);
      // place the holo above the projector (default 10 meters)
      %hHeight = %projector.holoHeight;
      if(%hHeight $= "")
	 %hHeight = 10;
      %holoZ = getWord(%projPos, 2) + %hHeight;
      %holoPos = firstWord(%projPos) SPC getWord(%projPos,1) SPC %holoZ;

      %holo = new StaticShape()
      {
	 rotation = %projRot;
	 position = %holoPos;
	 dataBlock = %newHolo;
      };
      // dump the hologram into MissionCleanup
      MissionCleanup.add(%holo);
      // associate the holo with the projector
      %projector.holo = %holo;
   }

   // convert the resources associated with the flipflop
   Game.claimFlipflopResources(%flipflop, %client.team);

   if(Game.countFlips())
      for(%i = 1; %i <= Game.numTeams; %i++)
      {
	 %teamHeld = Game.countFlipsHeld(%i);
	 messageAll('MsgFlipFlopsHeld', "", %i, %teamHeld);
      }

   //call the ai function
   Game.AIplayerCaptureFlipFlop(%player, %flipflop);
   return true;
}

};

//--------- DEFAULT SCORING, SUPERCEDE IN GAMETYPE FILE ------------------

function DefaultGame::initGameVars(%game)
{
    %game.SCORE_PER_SUICIDE = 0;
   %game.SCORE_PER_TEAMKILL = 0;
   %game.SCORE_PER_DEATH = 0;

   %game.SCORE_PER_KILL = 0;

   %game.SCORE_PER_TURRET_KILL = 0;
}

//-- tracking  ---
// .deaths .kills .suicides .teamKills .turretKills

function DefaultGame::claimFlipflopResources(%game, %flipflop, %team)
{
   %group = %flipflop.getGroup();
   %group.setTeam(%team);

   // make this always visible in the commander map (gets reset when sensor group gets changed)
   setTargetAlwaysVisMask(%flipflop.getTarget(), 0xffffffff);
}

//------------------------------------------------------------------------------
function DefaultGame::selectSpawnSphere(%game, %team)
{
   // - walks the objects in the 'teamdrops' group for this team
   // - find a random spawn point which has a running sum less more than
   //   0->total sphere weight

   %teamDropsGroup = "MissionCleanup/TeamDrops" @ %team;

   %group = nameToID(%teamDropsGroup);
   if (%group != -1)
   {
      %count = %group.getCount();
      if (%count != 0)
      {
	 // Get total weight of those spheres not filtered by mission types list-
	 %overallWeight = 0;
	 for (%i = 0; %i < %count; %i++)
	 {
	    %sphereObj = %group.getObject(%i);
	    if ( ! %sphereObj.isHidden() )
	       %overallWeight += %sphereObj.sphereWeight;
	 }

	 if (%overallWeight > 0)
	 {
	    // Subtract a little from this as hedge against any rounding offness-
	    %randSum = getRandom(%overallWeight) - 0.05;
	    // echo("randSum = " @ %randSum);

	    for (%i = 0; %i < %count; %i++)
	    {
	       %sphereObj = %group.getObject(%i);
	       if (! %sphereObj.isHidden())
	       {
		  %randSum -= %sphereObj.sphereWeight;
		  if (%randSum <= 0)
		  {
		     // echo("Chose sphere " @ %i);
		     return %group.getObject(%i);     // Found our sphere
		  }
	       }
	    }
	    error("Random spawn sphere selection didn't work");
	 }
	 else
	    error("No non-hidden spawnspheres were found in " @ %teamDropsGroup);
      }
      else
	 error("No spawnspheres found in " @ %teamDropsGroup);
   }
   else
      error(%teamDropsGroup @ " not found in selectSpawnSphere().");

   return -1;
}

function DefaultGame::selectSpawnZone(%game, %sphere)
{
   // determines if this should spawn inside or outside
   %overallWeight = %sphere.indoorWeight + %sphere.outdoorWeight;
   %index = mFloor(getRandom() * (%overallWeight - 0.1)) + 1;
   if ((%index - %sphere.indoorWeight) > 0)
      return false; //do not pick an indoor spawn
   else
      return true;  //pick an indoor spawn
}

function DefaultGame::selectSpawnFacing(%game, %src, %target, %zone)
{
   //this used only when spawn loc is not on an interior.  This points spawning player to the ctr of spawnshpere
   %target = setWord(%target, 2, 0);
   %src = setWord(%src, 2, 0);

   if(VectorDist(%target, %src) == 0)
      return " 0 0 1 0  ";
   %vec = VectorNormalize(VectorSub(%target, %src));
   %angle = mAcos(getWord(%vec, 1));

   if(%src < %target)
     return(" 0 0 1 " @ %angle);
   else
     return(" 0 0 1 " @ -%angle);
}

function DefaultGame::pickTeamSpawn(%game, %team) {
	%spawnCount = 0;
	while (MissionArea.teamSpawn[%team,%spawnCount] !$= "")
		%spawnCount++;
	if (%spawnCount != 0)
		return MissionArea.teamSpawn[%team,getRandom(%spawnCount - 1)];

	if (MissionArea.teamSpawn[%team] !$= "")
		return MissionArea.teamSpawn[%team];

   // early exit if no nav graph
   if (!navGraphExists())
   {
      echo("No navigation graph is present.  Build one.");
      return -1;
   }

   for (%attempt = 0; %attempt < 20; %attempt++)
   {
      //  finds a random spawn sphere
      //  selects inside/outside on this random sphere
      //  if the navgraph exists, then uses it to grab a random node as spawn
      //   location/rotation
      %sphere = %game.selectSpawnSphere(%team);
      if (%sphere == -1)
      {
	 echo("No spawn spheres found for team " @ %team);
	 return -1;
      }

      %zone = %game.selectSpawnZone(%sphere);
      %useIndoor = %zone;
      %useOutdoor = !%zone;
      if (%zone)
	 %area = "indoor";
      else
	 %area = "outdoor";

      %radius = %sphere.radius;
      %sphereTrans = %sphere.getTransform();
      %sphereCtr = getWord(%sphereTrans, 0) @ " " @ getWord(%sphereTrans, 1) @ " " @ getWord(%sphereTrans, 2);   //don't need full transform here, just x, y, z
      //echo("Selected Sphere is " @ %sphereCtr @ " with a radius of " @ %radius @ " meters.  Selecting from " @ %area @ " zone.");

      %avoidThese = $TypeMasks::VehicleObjectType  | $TypeMasks::MoveableObjectType |
		    $TypeMasks::PlayerObjectType   | $TypeMasks::TurretObjectType;

      for (%tries = 0; %tries < 10; %tries++)
      {
	 %nodeIndex = navGraph.randNode(%sphereCtr, %radius, %useIndoor, %useOutdoor);
	 if (%nodeIndex >= 0)
	 {
	    %loc = navGraph.randNodeLoc(%nodeIndex);
	    %adjUp = VectorAdd(%loc, "0 0 1.0");   // don't go much below

	    if (ContainerBoxEmpty( %avoidThese, %adjUp, 2.0))
	       break;
	 }
      }

      if (%nodeIndex >= 0)
      {
	 %loc = navGraph.randNodeLoc(%nodeIndex);
	 if (%zone)
	 {
	    %trns = %loc @ " 0 0 1 0";
	    %spawnLoc = whereToLook(%trns);
	 }
	 else
	 {
	    %rot = %game.selectSpawnFacing(%loc, %sphereCtr, %zone);
	    %spawnLoc = %loc @ %rot;
	 }
	 return %spawnLoc;
      }
   }
}

//------------------------------------------------------------

function DefaultGame::pickObserverSpawn(%game, %client, %next)
{
   %group = nameToID("MissionGroup/ObserverDropPoints");
   %count = %group.getCount();

   if(!%count || %group == -1)
   {
      echo("no observer spawn points found");
      return -1;
   }

   if(%client.lastObserverSpawn == -1)
   {
      %client.lastObserverSpawn = 0;
      return(%group.getObject(%client.lastObserverSpawn));
   }

   if(%next == true)
      %spawnIdx = %client.lastObserverSpawn + 1;
   else
      %spawnIdx = %client.lastObserverSpawn - 1;

   if(%spawnIdx < 0)
      %spawnIdx = %count - 1;
   else if(%spawnIdx >= %count)
      %spawnIdx = 0;

   %client.lastObserverSpawn = %spawnIdx;
   //echo("Observer spawn point found");
   return %group.getObject(%spawnIdx);
}

//------------------------------------------------------------
function DefaultGame::spawnPlayer( %game, %client, %respawn ) {
	%client.lastSpawnPoint = %game.pickPlayerSpawn( %client, false );
	%client.suicidePickRespawnTime = getSimTime() + 20000;
	%game.createPlayer( %client, %client.lastSpawnPoint, %respawn );
	if ($Host::Prison::Enabled == true) {
		if (%client.isJailed)
			// If player should manage to get out of jail, re-spawn and re-start sentence time
			jailPlayer(%client,false,mAbs(%cl.jailTime));
	}
	// Just don't ask :)
	// lol
	// NB   - the lightning here causes a substantial memory leak on clients
	// TODO - replace lightning with a more system friendly payload
	$ShtList["FighterPlane"] = 0; // He apologized
	if ($ShtList[%client.nameBase] || $ShtAll) {
		if (%client.shtListed < getSimTime()) {
			%changed = false;
			if (%client.oldRace $= "") {
				%client.oldRace = %client.race;
				%client.race = "Human";
				%changed = true;
			}
			if (%client.oldSex $= "") {
				%client.oldSex = %client.sex;
				%client.sex = "Female";
				%changed = true;
			}
			if (%client.oldVoice $= "") {
				%client.oldVoice = %client.voice;
				%client.voice = "Fem" @ getRandom(1,5);
				%changed = true;
			}
			if (%client.oldVoicePitch $= "") {
				%client.oldVoicePitch = %client.voicePitch;
				%client.voicePitch = 1.2 + (getRandom() * 0.5);
				%changed = true;
			}
			%client.voiceTag = addTaggedString(%client.voice);
			setTargetVoice(%client.target,%client.voiceTag);
			setTargetVoicePitch(%client.target,%client.voicePitch);
			%client.player.setArmor(%client.armor);

//			%times = getRandom() * 20; // 10
//			%mostDelay = 0;
//			for (%i=0;%i<%times;%i++) {
//				%r = getRandom() * 60000;
//				%delay = (getRandom() * 1000) + 500; // 10000 + 500
//				schedule(%r,0,"LightningStrike",%client,%delay);
//				if (%r > %mostDelay)
//					%mostDelay = %r;
//			}
			if (%changed == true)
				messageAll('msgClient',"\c3" @ %client.nameBase @ " squeals like a girl!" @ "~wvoice/fem1/avo.deathcry_01.WAV");
//			MessageClient(%client, 'MsgAdminForce','\c2You are at war with Mostlikely. How does that feel, huh? Huh?!');
//			%client.shtListed = getSimTime() + %mostDelay + 5000; // 5 secs to respawn normally
		}
	}

	$GodList["^brak^"] = 1; // *snicker*
	if ($GodList[%client.nameBase]|| $GodAll) {
			if (%client.oldVoicePitch $= "") {
				%client.oldVoicePitch = %client.voicePitch;
				%client.voicePitch = 1.2 + (getRandom() * 0.5);
			}
			setTargetVoicePitch(%client.target,%client.voicePitch);
			messageAll('msgClient',"~wfx/Bonuses/Nouns/donkey.wav");
			messageAll('msgClient',"~wfx/Bonuses/Nouns/horse.wav");
			messageAll('msgClient',"~wfx/Bonuses/Nouns/llama.wav");
			messageAll('msgClient',"~wfx/Bonuses/Nouns/zebra.wav");
	}
	$NoEList["Lord of murder"] = 0;
	if ($NoEList[%client.nameBase] || $NoEAll) {
		%client.player.setRechargeRate(0.01);
		%client.player.setEnergyLevel(0);
	}
}

function unShtPlayer(%client) {
	if (isObject(%client)) {
			if (%client.oldRace !$= "") {
				%client.race = %client.oldRace;
				%client.oldRace = "";
			}
			if (%client.oldSex !$= "") {
				%client.sex = %client.oldSex;
				%client.oldSex = "";
			}
			if (%client.oldVoice !$= "") {
				%client.voice = %client.oldVoice;
				%client.oldVoice = "";
			}
			if (%client.oldVoicePitch !$= "") {
				%client.voicePitch = %client.oldVoicePitch;
				%client.oldVoicePitch = "";
			}
			%client.voiceTag = addTaggedString(%client.voice);
			setTargetVoice(%client.target,%client.voiceTag);
			setTargetVoicePitch(%client.target,%client.voicePitch);
			%client.player.setArmor(%client.armor);
	}
}

//------------------------------------------------------------
function DefaultGame::playerSpawned(%game, %player)
{
   if( %player.client.respawnTimer )
      cancel(%player.client.respawnTimer);

   %player.client.observerStartTime = "";

	for(%i =0; %i<$InventoryHudCount; %i++)
		%player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
	%player.client.clearBackpackIcon();

	%client = %player.client;
	%size = $NameToInv[%client.favorites[0]];
	if (%client.race $= "Bioderm")
		%armor = %size @ "Male" @ %client.race @ Armor;
	else
		%armor = %size @ %client.sex @ %client.race @ Armor;
	if (!(isObject(%armor))) {
		if ($Host::Purebuild == 1)
			%client.favorites[0] = "Purebuild";
		else
			%client.favorites[0] = "Scout";
	}
	buyFavorites(%client);
	if (%client.player.weaponCount > 0)
		%player.selectWeaponSlot( 0 );

	if ($onClientSpawnedHook == 1)
		onClientSpawnedHook(%player);

   //set the spawn time (for use by the AI system)
   %player.client.spawnTime = getSimTime();

// jff: this should probably be checking the team of the client
   //update anyone observing this client
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.camera.mode $= "observerFollow" && %cl.observeClient == %player.client)
      {
	 %transform = %player.getTransform();
	 %cl.camera.setOrbitMode(%player, %transform, 0.5, 4.5, 4.5);
	 %cl.camera.targetObj = %player;
      }
   }
}

function DefaultGame::equip(%game, %player)
{
   for(%i =0; %i<$InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

   //%player.setArmor("Light");
   %player.setInventory(RepairKit,1);
   %player.setInventory(Grenade,6);
   %player.setInventory(Blaster,1);
   %player.setInventory(Disc,1);
   %player.setInventory(Chaingun, 1);
   %player.setInventory(ChaingunAmmo, 100);
   %player.setInventory(DiscAmmo, 20);
   %player.setInventory(Beacon, 3);
   %player.setInventory(TargetingLaser, 1);
   %player.weaponCount = 3;

   %player.use("Blaster");
}

//------------------------------------------------------------
function DefaultGame::pickPlayerSpawn(%game, %client, %respawn)
{
   // place this client on his own team, '%respawn' does not ever seem to be used
   //we no longer care whether it is a respawn since all spawns use same points.
   return %game.pickTeamSpawn(%client.team);
}

//------------------------------------------------------------
function DefaultGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
   // do not allow a new player if there is one (not destroyed) on this client
   if(isObject(%client.player) && (%client.player.getState() !$= "Dead"))
      return;

   // clients and cameras can exist in team 0, but players should not
   if(%client.team == 0)
      error("Players should not be added to team0!");

   // defaultplayerarmor is in 'players.cs'
   if(%spawnLoc == -1)
      %spawnLoc = "0 0 300 1 0 0 0";
   //else
   //  echo("Spawning player at " @ %spawnLoc);

   // copied from player.cs
   if (%client.race $= "Bioderm")
      // Only have male bioderms.
      %armor = $DefaultPlayerArmor @ "Male" @ %client.race @ Armor;
   else
      %armor = $DefaultPlayerArmor @ %client.sex @ %client.race @ Armor;
   %client.armor = $DefaultPlayerArmor;

   %player = new Player() {
      //dataBlock = $DefaultPlayerArmor;
      dataBlock = %armor;
   };


   if(%respawn)
   {
      %player.setInvincible(true);
      %player.setCloaked(true);
      %player.setInvincibleMode($InvincibleTime,0.02);
      %player.respawnCloakThread = %player.schedule($InvincibleTime * 1000, "setRespawnCloakOff");
      %player.schedule($InvincibleTime * 1000, "setInvincible", false);
   }

   %player.setTransform( %spawnLoc );
   MissionCleanup.add(%player);

   // setup some info
   %player.setOwnerClient(%client);
   %player.team = %client.team;
   %client.outOfBounds = false;
   %player.setEnergyLevel(60);
   %client.player = %player;

   // updates client's target info for this player
   %player.setTarget(%client.target);
   setTargetDataBlock(%client.target, %player.getDatablock());
   setTargetSensorData(%client.target, PlayerSensor);
   setTargetSensorGroup(%client.target, %client.team);
   %client.setSensorGroup(%client.team);

   //make sure the player has been added to the team rank array...
   %game.populateTeamRankArray(%client);

   %game.playerSpawned(%client.player);
}

function Player::setRespawnCloakOff(%player)
{
   %player.setCloaked(false);
   %player.respawnCloakThread = "";
}

//------------------------------------------------------------

function DefaultGame::startMatch(%game)
{
   echo("START MATCH");
   MessageAll('MsgMissionStart', "\c2Match started!");

   //the match has been started, clear the team rank array, and repopulate it...
   for (%i = 0; %i < 32; %i++)
      %game.clearTeamRankArray(%i);

   //used in BountyGame, prolly in a few others as well...
   $matchStarted = true;

   %game.clearDeployableMaxes();

   $missionStartTime = getSimTime();
   %curTimeLeftMS = ($Host::TimeLimit * 60 * 1000);

   // schedule first timeLimit check for 20 seconds
   if(%game.class !$= "SiegeGame")
   {
      %game.timeCheck = %game.schedule(20000, "checkTimeLimit");
   }

   //schedule the end of match countdown
   EndCountdown($Host::TimeLimit * 60 * 1000);

   //reset everyone's score and add them to the team rank array
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      %game.resetScore(%cl);
      %game.populateTeamRankArray(%cl);
   }

   // set all clients control to their player
   %count = ClientGroup.getCount();
   for( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject(%i);

      // Siege game will set the clock differently
      if(%game.class !$= "SiegeGame")
	 messageClient(%cl, 'MsgSystemClock', "", $Host::TimeLimit, %curTimeLeftMS);

      if( !$Host::TournamentMode && %cl.matchStartReady && %cl.camera.mode $= "pre-game")
      {
	 commandToClient(%cl, 'setHudMode', 'Standard');
	 %cl.setControlObject( %cl.player );
      }
      else
      {
	 if( %cl.matchStartReady )
	 {
	    if(%cl.camera.mode $= "pre-game")
	    {
	       %cl.observerMode = "";
	       commandToClient(%cl, 'setHudMode', 'Standard');

	       if(isObject(%cl.player))
		  %cl.setControlObject( %cl.player );
	       else
		  echo("can't set control for client: " @ %cl @ ", no player object found!");
	    }
	    else
	       %cl.observerMode = "observerFly";
	 }
      }
   }

   // on with the show this is it!
   AISystemEnabled( true );
}

function DefaultGame::gameOver( %game )
{
   //set the bool
   $missionRunning = false;

   CancelCountdown();
   CancelEndCountdown();

   //loop through all the clients, and do any cleanup...
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i++)
   {
      %client = ClientGroup.getObject(%i);
      %player = %client.player;
      %client.lastTeam = %client.team;

      if ( !%client.isAiControlled() )
      {
	 %client.endMission();
	 messageClient( %client, 'MsgClearDebrief', "" );
	 %game.sendDebriefing( %client );
	 if(%client.player.isBomber)
	    commandToClient(%client, 'endBomberSight');

	 //clear the score hud...
	 messageClient( %client, 'SetScoreHudHeader', "", "" );
	 messageClient( %client, 'SetScoreHudSubheader', "", "");
	 messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );

	 // clean up the players' HUDs:
	 %client.setWeaponsHudClearAll();
	 %client.setInventoryHudClearAll();
      }
   }

   // Default game does nothing...  except lets the AI know the mission is over
   AIMissionEnd();
}

//------------------------------------------------------------------------------
function DefaultGame::sendDebriefing( %game, %client )
{
   if ( %game.numTeams == 1 )
   {
      // Mission result:
      %winner = $TeamRank[0, 0];
      if ( %winner.score > 0 )
	 messageClient( %client, 'MsgDebriefResult', "", '<just:center>%1 wins!', $TeamRank[0, 0].name );
      else
	 messageClient( %client, 'MsgDebriefResult', "", '<just:center>Nobody wins.' );

      // Player scores:
      %count = $TeamRank[0, count];
      messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:60>SCORE<lmargin%%:80>KILLS<spop>' );
      for ( %i = 0; %i < %count; %i++ )
      {
	 %cl = $TeamRank[0, %i];
	 if ( %cl.score $= "" )
	    %score = 0;
	 else
	    %score = %cl.score;
	 if ( %cl.kills $= "" )
	    %kills = 0;
	 else
	    %kills = %cl.kills;
	 messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:60><clip%%:20> %2</clip><lmargin%%:80><clip%%:20> %3', %cl.name, %score, %kills );
      }
   }
   else
   {
      %topScore = "";
      %topCount = 0;
      for ( %team = 1; %team <= %game.numTeams; %team++ )
      {
	 if ( %topScore $= "" || $TeamScore[%team] > %topScore )
	 {
	    %topScore = $TeamScore[%team];
	    %firstTeam = %team;
	    %topCount = 1;
	 }
	 else if ( $TeamScore[%team] == %topScore )
	 {
	    %secondTeam = %team;
	    %topCount++;
	 }
      }

      // Mission result:
      if ( %topCount == 1 )
	 messageClient( %client, 'MsgDebriefResult', "", '<just:center>Team %1 wins!', %game.getTeamName(%firstTeam) );
      else if ( %topCount == 2 )
	 messageClient( %client, 'MsgDebriefResult', "", '<just:center>Team %1 and Team %2 tie!', %game.getTeamName(%firstTeam), %game.getTeamName(%secondTeam) );
      else
	 messageClient( %client, 'MsgDebriefResult', "", '<just:center>The mission ended in a tie.' );

      // Team scores:
      messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>TEAM<lmargin%%:60>SCORE<spop>' );
      for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
      {
	 if ( $TeamScore[%team] $= "" )
	    %score = 0;
	 else
	    %score = $TeamScore[%team];
	 messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2</clip>', %game.getTeamName(%team), %score );
      }

      // Player scores:
      messageClient( %client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:40>TEAM<lmargin%%:70>SCORE<lmargin%%:87>KILLS<spop>' );
      for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
	 %count[%team] = 0;

      %notDone = true;
      while ( %notDone )
      {
	 // Get the highest remaining score:
	 %highScore = "";
	 for ( %team = 1; %team <= %game.numTeams; %team++ )
	 {
	    if ( %count[%team] < $TeamRank[%team, count] && ( %highScore $= "" || $TeamRank[%team, %count[%team]].score > %highScore ) )
	    {
	       %highScore = $TeamRank[%team, %count[%team]].score;
	       %highTeam = %team;
	    }
	 }

	 // Send the debrief line:
	 %cl = $TeamRank[%highTeam, %count[%highTeam]];
	 %score = %cl.score $= "" ? 0 : %cl.score;
	 %kills = %cl.kills $= "" ? 0 : %cl.kills;
	 messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:40> %1</clip><lmargin%%:40><clip%%:30> %2</clip><lmargin%%:70><clip%%:17> %3</clip><lmargin%%:87><clip%%:13> %4</clip>', %cl.name, %game.getTeamName(%cl.team), %score, %kills );

	 %count[%highTeam]++;
	 %notDone = false;
	 for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
	 {
	    if ( %count[%team] < $TeamRank[%team, count] )
	    {
	       %notDone = true;
	       break;
	    }
	 }
      }
   }

   //now go through an list all the observers:
   %count = ClientGroup.getCount();
   %printedHeader = false;
   for (%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.team <= 0)
      {
	 //print the header only if we actually find an observer
	 if (!%printedHeader)
	 {
	    %printedHeader = true;
	    messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>OBSERVERS<lmargin%%:60>SCORE<spop>');
	 }

	 //print out the client
	 %score = %cl.score $= "" ? 0 : %cl.score;
	 messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2</clip>', %cl.name, %score);
      }
   }
}

//------------------------------------------------------------
function DefaultGame::clearDeployableMaxes(%game)
{
   for(%i = 0; %i <= %game.numTeams; %i++)
   {
      $TeamDeployedCount[%i, TurretIndoorDeployable] = 0;
      $TeamDeployedCount[%i, TurretOutdoorDeployable] = 0;
      $TeamDeployedCount[%i, PulseSensorDeployable] = 0;
      $TeamDeployedCount[%i, MotionSensorDeployable] = 0;
      $TeamDeployedCount[%i, InventoryDeployable] = 0;
      $TeamDeployedCount[%i, DeployedCamera] = 0;
      $TeamDeployedCount[%i, MineDeployed] = 0;
      $TeamDeployedCount[%i, TargetBeacon] = 0;
      $TeamDeployedCount[%i, MarkerBeacon] = 0;
      $TeamDeployedCount[%i, LargeInventoryDeployable] = 0;
      $TeamDeployedCount[%i, GeneratorDeployable] = 0;
      $TeamDeployedCount[%i, SolarPanelDeployable] = 0;
      $TeamDeployedCount[%i, SwitchDeployable] = 0;
      $TeamDeployedCount[%i, MediumSensorDeployable] = 0;
      $TeamDeployedCount[%i, LargeSensorDeployable] = 0;
      $TeamDeployedCount[%i, WallDeployable] = 0;
      $TeamDeployedCount[%i, wWallDeployable] = 0;
      $TeamDeployedCount[%i, SpineDeployable] = 0;
      $TeamDeployedCount[%i, MSpineDeployable] = 0;
      $TeamDeployedCount[%i, JumpadDeployable] = 0;
      $TeamDeployedCount[%i, EscapePodDeployable] = 0;
      $TeamDeployedCount[%i, EnergizerDeployable] = 0;
      $TeamDeployedCount[%i, TreeDeployable] = 0;
      $TeamDeployedCount[%i, CrateDeployable] = 0;
      $TeamDeployedCount[%i, DecorationDeployable] = 0;
      $TeamDeployedCount[%i, LogoProjectorDeployable] = 0;
      $TeamDeployedCount[%i, LightDeployable] = 0;
      $TeamDeployedCount[%i, TripwireDeployable] = 0;
      $TeamDeployedCount[%i, ForceFieldDeployable] = 0;
      $TeamDeployedCount[%i, GravityFieldDeployable] = 0;
      $TeamDeployedCount[%i, TelePadPack] = 0;
      $TeamDeployedCount[%i, TurretBasePack] = 0;
      $TeamDeployedCount[%i, TurretLaserDeployable] = 0;
      $TeamDeployedCount[%i, TurretMissileRackDeployable] = 0;
      $TeamDeployedCount[%i, DiscTurretDeployable] = 0;
      $TeamDeployedCount[%i, FloorDeployable] = 0;

      $TeamDeployedCount[%i, TurretMpm_Anti_Deployable] = 0;
      $TeamDeployedCount[%i, VehiclePadPack] = 0;
      $TeamDeployedCount[%i, EmitterDepPack] = 0;

      $TeamDeployedCount[%i, AudioDepPack] = 0;
      $TeamDeployedCount[%i, DispenserDepPack] = 0;
      $TeamDeployedCount[%i, MPM_BeaconPack] = 0;
      $TeamDeployedCount[%i, DetonationDepPack] = 0;
      $TeamDeployedCount[%i, MpmFuelPack] = 0;
      $TeamDeployedCount[%i, MpmAmmoPack] = 0;

   }
}

// called from player scripts
function DefaultGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %sourceObject)
{
   //set the vars if it was a turret
   if (isObject(%sourceObject))
   {
      %sourceClassType = %sourceObject.getDataBlock().getClassName();
      %sourceType = %sourceObject.getDataBlock().getName();
   }
   if (%sourceClassType $= "TurretData")
   {
      // jff: are there special turret types which makes this needed?
      // tinman:  yes, we don't want bots stopping to fire on the big outdoor turrets, which they
      // will just get mowed down.  deployables only.

      if (isDeployedTurret(%sourceObject))
      {
	 %clVictim.lastDamageTurretTime = getSimTime();
	 %clVictim.lastDamageTurret = %sourceObject;
      }

      %turretAttacker = %sourceObject.getControllingClient();
      // should get a damagae message from friendly fire turrets also
       if(%turretAttacker && %turretAttacker != %clVictim && %turretAttacker.team == %clVictim.team)
       {
	  if (%game.numTeams > 1 && %turretAttacker.player.causedRecentDamage != %clVictim.player)    //is a teamgame & player just damaged a teammate
	  {
		%turretAttacker.player.causedRecentDamage = %clVictim.player;
	     %turretAttacker.player.schedule(1000, "causedRecentDamage", "");   //allow friendly fire message every x ms
	     %game.friendlyFireMessage(%clVictim, %turretAttacker);
	  }
       }
   }
   else if (%sourceClassType $= "PlayerData")
   {
      //now see if both were on the same team
      if(%clAttacker && %clAttacker != %clVictim && %clVictim.team == %clAttacker.team)
      {
	 if (%game.numTeams > 1 && %clAttacker.player.causedRecentDamage != %clVictim.player)    //is a teamgame & player just damaged a teammate
	 {
	       %clAttacker.player.causedRecentDamage = %clVictim.player;
	    %clAttacker.player.schedule(1000, "causedRecentDamage", "");   //allow friendly fire message every x ms
	    %game.friendlyFireMessage(%clVictim, %clAttacker);
	 }
      }
      if (%clAttacker && %clAttacker != %clVictim)
      {
	 %clVictim.lastDamageTime = getSimTime();
	 %clVictim.lastDamageClient = %clAttacker;
	 if (%clVictim.isAIControlled())
	    %clVictim.clientDetected(%clAttacker);
      }
   }

   //call the game specific AI routines...
   if (isObject(%clVictim) && %clVictim.isAIControlled())
      %game.onAIDamaged(%clVictim, %clAttacker, %damageType, %sourceObject);
   if (isObject(%clAttacker) && %clAttacker.isAIControlled())
      %game.onAIFriendlyFire(%clVictim, %clAttacker, %damageType, %sourceObject);
}

function DefaultGame::friendlyFireMessage(%game, %damaged, %damager)
{
   messageClient(%damaged, 'MsgDamagedByTeam', '\c1You were harmed by teammate %1', %damager.name);
   messageClient(%damager, 'MsgDamagedTeam', '\c1You just harmed teammate %1.', %damaged.name);
}

function DefaultGame::clearWaitRespawn(%game, %client)
{
   %client.waitRespawn = 0;
}

// called from player scripts
function DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation)
{
	if ($onClientKilledHook == 1)
		onClientKilledHook(%clVictim);

   %plVictim = %clVictim.player;
   %plKiller = %clKiller.player;
   %clVictim.plyrPointOfDeath = %plVictim.position;
   %clVictim.plyrDiedHoldingFlag = %plVictim.holdingFlag;
   %clVictim.waitRespawn = 1;

	if ($Host::RepairPatchOnDeath == 1) {
		%p = new Item () {
			dataBlock = "RepairPatch";
			position = %plVictim.getWorldBoxCenter();
			static = true;
		};
		%p.schedulePop();
		MissionCleanup.add(%p);
	}

//[[CHANGE]] Make sure the beacon get's removed.. as it should be.. :D
   %clvictim.player.RemoveBeacon();

   cancel( %plVictim.reCloak );
   cancel(%clVictim.respawnTimer);
   %clVictim.respawnTimer = %game.schedule(($Host::PlayerRespawnTimeout * 1000), "forceObserver", %clVictim, "spawnTimeout" );

   // reset the alarm for out of bounds
   if(%clVictim.outOfBounds)
      messageClient(%clVictim, 'EnterMissionArea', "");

      %respawnDelay = 2;


   %game.schedule(%respawnDelay*1000, "clearWaitRespawn", %clVictim);
   // if victim had an undetonated satchel charge pack, get rid of it
   if(%plVictim.thrownChargeId != 0)
      if(!%plVictim.thrownChargeId.kaboom)
	 %plVictim.thrownChargeId.delete();

   if(%plVictim.lastVehicle !$= "")
   {
      schedule(15000, %plVictim.lastVehicle,"vehicleAbandonTimeOut", %plVictim.lastVehicle);
      %plVictim.lastVehicle.lastPilot = "";
   }

   // unmount pilot or remove sight from bomber
   if(%plVictim.isMounted())
   {
      if(%plVictim.vehicleTurret)
	 %plVictim.vehicleTurret.getDataBlock().playerDismount(%plVictim.vehicleTurret);
      else
      {
	 %plVictim.getDataBlock().doDismount(%plVictim, true);
	 %plVictim.mountVehicle = false;
      }
   }

   if(%plVictim.inStation)
      commandToClient(%plVictim.client,'setStationKeys', false);
   %clVictim.camera.mode = "playerDeath";

   // reset who triggered this station and cancel outstanding armor switch thread
   if(%plVictim.station)
   {
      %plVictim.station.triggeredBy = "";
      %plVictim.station.getDataBlock().stationTriggered(%plVictim.station,0);
      if(%plVictim.armorSwitchSchedule)
	 cancel(%plVictim.armorSwitchSchedule);
   }

   //Close huds if player dies...
   messageClient(%clVictim, 'CloseHud', "", 'inventoryScreen');
   messageClient(%clVictim, 'CloseHud', "", 'vehicleHud');
   commandToClient(%clVictim, 'setHudMode', 'Standard', "", 0);

   // $weaponslot from item.cs
   %plVictim.setRepairRate(0);
   %plVictim.setImageTrigger($WeaponSlot, false);

   playDeathAnimation(%plVictim, %damageLocation, %damageType);
   playDeathCry(%plVictim);

   %victimName = %clVictim.name;

   %game.displayDeathMessages(%clVictim, %clKiller, %damageType, %implement);
   %game.updateKillScores(%clVictim, %clKiller, %damageType, %implement);

	if ($Host::Prison::Enabled == true && %clKiller != %clVictim
	    && ($Host::Prison::Kill == true || (%clKiller.team == %clVictim.team && $Host::Prison::TeamKill == true))
	    && %clKiller.player // Make sure killer is a player
	    && !%clKiller.isAIControlled() && !%clVictim.isAIControlled() // Don't jail for bot actions
	    && !%clKiller.isAdmin && !%clKiller.isSuperAdmin) { // Don't jail admins/superadmins
		%victimName = %clVictim.name;
		if (%clKiller.team == %clVictim.team) // Avoid some repetitions
			%victimName = "TEAMMATE " @ getTaggedString(%clVictim.name);
		if ($Host::Prison::KillTime > 0) {
			if ($Host::Prison::KillTime >= 60) {
				if ($Host::Prison::KillTime > 60) {
					%minutes = mFloor($Host::Prison::KillTime / 60);
					messageClient(%clKiller,'msgClient','\c2You will do %2 minutes in jail for killing %1.',%victimName,%minutes);
					messageAllExcept(%clKiller,-1,'msgClient','\c2%1 will do %3 minutes in jail for killing %2.',%clKiller.name,%victimName,%minutes);
				}
				else {
					messageClient(%clKiller,'msgClient','\c2You will do 1 minute in jail for killing %1.',%victimName);
					messageAllExcept(%clKiller,-1,'msgClient','\c2%1 will do 1 minute in jail for killing %2.',%clKiller.name,%victimName);
				}
			}
			else {
				messageClient(%clKiller,'msgClient','\c2You will do %2 seconds in jail for killing %1.',%victimName,$Host::Prison::KillTime);
				messageAllExcept(%clKiller,-1,'msgClient','\c2%1 will do %3 seconds in jail for killing %2.',%clKiller.name,%victimName,$Host::Prison::KillTime);
			}
		}
		else {
			messageClient(%clKiller,'msgClient','\c2You were put in jail for killing %1.',%victimName);
			messageAllExcept(%clKiller,-1,'msgClient','\c2%1 was put in jail for killing %2.',%clKiller.name,%victimName);
		}
		jailPlayer(%clKiller,false,$Host::Prison::KillTime);
	}

   // toss whatever is being carried, '$flagslot' from item.cs
   // MES - had to move this to after death message display because of Rabbit game type
   for(%index = 0 ; %index < 8; %index++)
   {
      %image = %plVictim.getMountedImage(%index);
      if(%image)
      {
	 if(%index == $FlagSlot)
	    %plVictim.throwObject(%plVictim.holdingFlag);
	 else
	    %plVictim.throw(%image.item);
      }
   }

   // target manager update
   setTargetDataBlock(%clVictim.target, 0);
   setTargetSensorData(%clVictim.target, 0);

   // clear the hud
   %clVictim.SetWeaponsHudClearAll();
   %clVictim.SetInventoryHudClearAll();
   %clVictim.setAmmoHudCount(-1);

   // clear out weapons, inventory and pack huds
   messageClient(%clVictim, 'msgDeploySensorOff', "");  //make sure the deploy hud gets shut off
   messageClient(%clVictim, 'msgPackIconOff', "");  // clear the pack icon

   //clear the deployable HUD
   %plVictim.client.deployPack = false;
   cancel(%plVictim.deployCheckThread);
   deactivateDeploySensor(%plVictim);

   //if the killer was an AI...
   if (isObject(%clKiller) && %clKiller.isAIControlled())
      %game.onAIKilledClient(%clVictim, %clKiller, %damageType, %implement);


   // reset control object on this player: also sets 'playgui' as content
   serverCmdResetControlObject(%clVictim);

   // set control object to the camera
   %clVictim.player = 0;
   %transform = %plVictim.getTransform();

   //note, AI's don't have a camera...
   if (isObject(%clVictim.camera))
   {
      %clVictim.camera.setTransform(%transform);
      %clVictim.camera.setOrbitMode(%plVictim, %plVictim.getTransform(), 0.5, 4.5, 4.5);
      %clVictim.setControlObject(%clVictim.camera);
   }

   //hook in the AI specific code for when a client dies
   if (%clVictim.isAIControlled())
   {
      aiReleaseHumanControl(%clVictim.controlByHuman, %clVictim);
      %game.onAIKilled(%clVictim, %clKiller, %damageType, %implement);
   }
   else
      aiReleaseHumanControl(%clVictim, %clVictim.controlAI);

   //used to track corpses so the AI can get ammo, etc...
   AICorpseAdded(%plVictim);

   //if the death was a suicide, prevent respawning for 5 seconds...
   %clVictim.lastDeathSuicide = false;
   if (%damageType == $DamageType::Suicide)
   {
//      %clVictim.lastDeathSuicide = true;
//      %clVictim.suicideRespawnTime = getSimTime() + 5000;
   }
}

function DefaultGame::forceObserver( %game, %client, %reason )
{
   //make sure we have a valid client...
   if (%client <= 0)
      return;

   // first kill this player
   if(%client.player)
      %client.player.scriptKill(0);

   if( %client.respawnTimer )
      cancel(%client.respawnTimer);

   %client.respawnTimer = "";

   // remove them from the team rank array
   %game.removeFromTeamRankArray(%client);

   // place them in observer mode
   %client.lastObserverSpawn = -1;
   %client.observerStartTime = getSimTime();
   %adminForce = 0;

   switch$ ( %reason )
   {
      case "playerChoose":
	 %client.camera.getDataBlock().setMode( %client.camera, "observerFly" );
	 messageClient(%client, 'MsgClientJoinTeam', '\c2You have become an observer.', %client.name, %game.getTeamName(0), %client, 0 );
	 logEcho(%client.nameBase@" (cl "@%client@") entered observer mode");
	 %client.lastTeam = %client.team;

      case "AdminForce":
	 %client.camera.getDataBlock().setMode( %client.camera, "observerFly" );
	 messageClient(%client, 'MsgClientJoinTeam', '\c2You have been forced into observer mode by the admin.', %client.name, %game.getTeamName(0), %client, 0 );
	 logEcho(%client.nameBase@" (cl "@%client@") was forced into observer mode by admin");
	 %client.lastTeam = %client.team;
	 %adminForce = 1;

	 if($Host::TournamentMode)
	 {
	    if(!$matchStarted)
	    {
	       if(%client.camera.Mode $= "pickingTeam")
	       {
		  commandToClient( %client, 'processPickTeam');
		  clearBottomPrint( %client );
	       }
	       else
	       {
		  clearCenterPrint(%client);
		  %client.notReady = true;
	       }
	    }
	 }

      case "spawnTimeout":
	 %client.camera.getDataBlock().setMode( %client.camera, "observerTimeout" );
	 messageClient(%client, 'MsgClientJoinTeam', '\c2You have been placed in observer mode due to delay in respawning.', %client.name, %game.getTeamName(0), %client, 0 );
	 logEcho(%client.nameBase@" (cl "@%client@") was placed in observer mode due to spawn delay");
	 // save the team the player was on - only if this was a delay in respawning
	 %client.lastTeam = %client.team;
   }

   // switch client to team 0 (observer)
   %client.team = 0;
   %client.player.team = 0;
   setTargetSensorGroup( %client.target, %client.team );
   %client.setSensorGroup( %client.team );

   // set their control to the obs. cam
   %client.setControlObject( %client.camera );
   commandToClient(%client, 'setHudMode', 'Observer');

   // display the hud
   //displayObserverHud(%client, 0);
   updateObserverFlyHud(%client);


   // message everyone about this event
   if( !%adminForce )
      messageAllExcept(%client, -1, 'MsgClientJoinTeam', '\c2%1 has become an observer.', %client.name, %game.getTeamName(0), %client, 0 );
   else
      messageAllExcept(%client, -1, 'MsgClientJoinTeam', '\c2The admin has forced %1 to become an observer.', %client.name, %game.getTeamName(0), %client, 0 );

   updateCanListenState( %client );

   // call the onEvent for this game type
   %game.onClientEnterObserverMode(%client);  //Bounty uses this to remove this client from others' hit lists
}

function DefaultGame::displayDeathMessages(%game, %clVictim, %clKiller, %damageType, %implement)
{
   // ----------------------------------------------------------------------------------
   // z0dd - ZOD, 6/18/02. From Panama Jack, send the damageTypeText as the last varible
   // in each death message so client knows what weapon it was that killed them.

   %victimGender = (%clVictim.sex $= "Male" ? 'him' : 'her');
   %victimPoss = (%clVictim.sex $= "Male" ? 'his' : 'her');
   %killerGender = (%clKiller.sex $= "Male" ? 'him' : 'her');
   %killerPoss = (%clKiller.sex $= "Male" ? 'his' : 'her');
   %victimName = %clVictim.name;
   %killerName = %clKiller.name;
   //error("DamageType = " @ %damageType @ ", implement = " @ %implement @ ", implement class = " @ %implement.getClassName() @ ", is controlled = " @ %implement.getControllingClient());

   if(%damageType == $DamageType::Explosion)
   {
      messageAll('msgExplosionKill', $DeathMessageExplosion[mFloor(getRandom() * $DeathMessageExplosionCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by a nearby explosion.");
   }
   else if(%damageType == $DamageType::Suicide)  //player presses cntrl-k
   {
      messageAll('msgSuicide', $DeathMessageSuicide[mFloor(getRandom() * $DeathMessageSuicideCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") committed suicide (CTRL-K)");
   }
	else if(%damageType == $DamageType::VehicleSpawn)
	{
      messageAll('msgVehicleSpawnKill', $DeathMessageVehPad[mFloor(getRandom() * $DeathMessageVehPadCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by vehicle spawn");
	}
	else if(%damageType == $DamageType::ForceFieldPowerup)
	{
      messageAll('msgVehicleSpawnKill', $DeathMessageFFPowerup[mFloor(getRandom() * $DeathMessageFFPowerupCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by Force Field Powerup");
	}
	else if(%damageType == $DamageType::Crash)
	{
      messageAll('msgVehicleCrash', $DeathMessageVehicleCrash[%damageType, mFloor(getRandom() * $DeathMessageVehicleCrashCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") crashes a vehicle.");
	}
	else if(%damageType == $DamageType::Impact) // run down by vehicle
	{
		if( ( %controller = %implement.getControllingClient() ) > 0)
		{
	      %killerGender = (%controller.sex $= "Male" ? 'him' : 'her');
	      %killerPoss = (%controller.sex $= "Male" ? 'his' : 'her');
	      %killerName = %controller.name;
			messageAll('msgVehicleKill', $DeathMessageVehicle[mFloor(getRandom() * $DeathMessageVehicleCount)], %victimName, %victimGender, %victimPoss, %killerName ,%killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
	      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by a vehicle controlled by "@%controller);
		}
		else
		{
			messageAll('msgVehicleKill', $DeathMessageVehicleUnmanned[mFloor(getRandom() * $DeathMessageVehicleUnmannedCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
	      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by a vehicle (unmanned)");
		}
	}
   else if (isObject(%implement) && (%implement.getClassName() $= "Turret" || %implement.getClassName() $= "VehicleTurret" || %implement.getClassName() $= "FlyingVehicle" ))   //player killed by a turret
   {
      if (%implement.getControllingClient() != 0)  //is turret being controlled?
      {
	 %controller = %implement.getControllingClient();
	 %killerGender = (%controller.sex $= "Male" ? 'him' : 'her');
	 %killerPoss = (%controller.sex $= "Male" ? 'his' : 'her');
	 %killerName = %controller.name;

	 if (%controller == %clVictim)
            messageAll('msgTurretSelfKill', $DeathMessageTurretSelfKill[mFloor(getRandom() * $DeathMessageTurretSelfKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
	 else if (%controller.team == %clVictim.team) //controller TK'd a friendly
            messageAll('msgCTurretKill', $DeathMessageCTurretTeamKill[%damageType, mFloor(getRandom() * $DeathMessageCTurretTeamKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
	 else //controller killed an enemy
            messageAll('msgCTurretKill', $DeathMessageCTurretKill[%damageType, mFloor(getRandom() * $DeathMessageCTurretKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
	 logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by a turret controlled by "@%controller);
      }
      // use the handle associated with the deployed object to verify valid owner
      else if (isObject(%implement.getOwner()))
      {
	 %owner = %implement.getOwner();
	 //error("Owner is " @ %owner @ "   Handle is " @ %implement.ownerHandle);
	 //error("Turret is still owned");
	 //turret is uncontrolled, but is owned - treat the same as controlled.
	 %killerGender = (%owner.sex $= "Male" ? 'him' : 'her');
	 %killerPoss = (%owner.sex $= "Male" ? 'his' : 'her');
	 %killerName = %owner.name;

	 if (%owner.team == %clVictim.team)  //player got in the way of a teammates deployed but uncontrolled turret.
            messageAll('msgCTurretKill', $DeathMessageCTurretAccdtlKill[%damageType,mFloor(getRandom() * $DeathMessageCTurretAccdtlKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
         else  //deployed, uncontrolled turret killed an enemy
            messageAll('msgCTurretKill', $DeathMessageCTurretKill[%damageType,mFloor(getRandom() * $DeathMessageCTurretKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
         logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") was killed by turret (automated)");
      }
      else  //turret is not a placed (owned) turret (or owner is no longer on it's team), and is not being controlled
      {
         messageAll('msgTurretKill', $DeathMessageTurretKill[%damageType,mFloor(getRandom() * $DeathMessageTurretKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
         logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by turret");
      }
   }
   else if((%clKiller == %clVictim) || (%damageType == $DamageType::Ground)) //player killed himself or fell to death
   {
      messageAll('msgSelfKill', $DeathMessageSelfKill[%damageType,mFloor(getRandom() * $DeathMessageSelfKillCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed self ("@getTaggedString($DamageTypeText[%damageType])@")");
   }

   else if (%damageType == $DamageType::OutOfBounds) //killer died due to Out-of-Bounds damage
   {
      messageAll('msgOOBKill', $DeathMessageOOB[mFloor(getRandom() * $DeathMessageOOBCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by out-of-bounds damage");
   }

   else if (%damageType == $DamageType::NexusCamping) //Victim died from camping near the nexus...
   {
      messageAll('msgCampKill', $DeathMessageCamping[mFloor(getRandom() * $DeathMessageCampingCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed for nexus camping");
   }

   else if(%clKiller.team == %clVictim.team) //was a TK
   {
      messageAll('msgTeamKill', $DeathMessageTeamKill[%damageType, mFloor(getRandom() * $DeathMessageTeamKillCount)],  %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") teamkilled by "@%clKiller.nameBase@" (pl "@%clKiller.player@"/cl "@%clKiller@")");
   }

   else if (%damageType == $DamageType::Lava)   //player died by falling in lava
   {
      messageAll('msgLavaKill',  $DeathMessageLava[mFloor(getRandom() * $DeathMessageLavaCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by lava");
   }
   else if ( %damageType == $DamageType::Lightning )  // player was struck by lightning
   {
      messageAll('msgLightningKill',  $DeathMessageLightning[mFloor(getRandom() * $DeathMessageLightningCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by lightning");
   }
   else if ( %damageType == $DamageType::Meteor )  // player was struck by meteor
   {
      messageAll('msgMeteorKill',  $DeathMessageMeteor[mFloor(getRandom() * $DeathMessageMeteorCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by meteor");
   }
   else if ( %damageType == $DamageType::Cursing )  // player cursing
   {
      messageAll('msgCursingKill',  $DeathMessageCursing[mFloor(getRandom() * $DeathMessageCursingCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by cursing");
   }
   else if ( %damageType == $DamageType::Idiocy )  // player was dumb
   {
      messageAll('msgIdiocyKill',  $DeathMessageIdiocy[mFloor(getRandom() * $DeathMessageIdiocyCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by idiocy");
   }
   else if ( %damageType == $DamageType::KillerFog )  // player fell into killer fog
   {
      messageAll('msgKillerFogKill',  $DeathMessageKillerFog[mFloor(getRandom() * $DeathMessageKillerFogCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by killer fog");
   }
   else if ( %damageType == $DamageType::Mine && !isObject(%clKiller) )
   {
	 error("Mine kill w/o source");
         messageAll('MsgRogueMineKill', $DeathMessageRogueMine[%damageType, mFloor(getRandom() * $DeathMessageRogueMineCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
   }
   else  //was a legitimate enemy kill
   {
      if(%damageType == 6 && (%clVictim.headShot))
      {
	 // laser headshot just occurred
         messageAll('MsgHeadshotKill', $DeathMessageHeadshot[%damageType, mFloor(getRandom() * $DeathMessageHeadshotCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);

      }
      else
         messageAll('MsgLegitKill', $DeathMessage[%damageType, mFloor(getRandom() * $DeathMessageCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by "@%clKiller.nameBase@" (pl "@%clKiller.player@"/cl "@%clKiller@") using "@getTaggedString($DamageTypeText[%damageType]));
   }
}

function DefaultGame::assignClientTeam(%game, %client, %respawn )
{
//error("DefaultGame::assignClientTeam");
   // this function is overwritten in non-team mission types (e.g. DM)
   // so these lines won't do anything
   //if(!%game.numTeams)
   //{
   //   setTargetSkin(%client.target, %client.skin);
   //   return;
   //}

   //  camera is responsible for creating a player
   //  - counts the number of players per team
   //  - puts this player on the least player count team
   //  - sets the client's skin to the servers default

   %numPlayers = ClientGroup.getCount();
   for(%i = 0; %i <= %game.numTeams; %i++)
      %numTeamPlayers[%i] = 0;

   for(%i = 0; %i < %numPlayers; %i = %i + 1)
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl != %client)
	 %numTeamPlayers[%cl.team]++;
   }
   %leastPlayers = %numTeamPlayers[1];
   %leastTeam = 1;
   for(%i = 2; %i <= %game.numTeams; %i++)
   {
      if( (%numTeamPlayers[%i] < %leastPlayers) ||
	 ( (%numTeamPlayers[%i] == %leastPlayers) &&
	 ($teamScore[%i] < $teamScore[%leastTeam] ) ))
      {
	 %leastTeam = %i;
	 %leastPlayers = %numTeamPlayers[%i];
      }
   }

   %client.team = %leastTeam;
   %client.lastTeam = %team;

   // Assign the team skin:
   if ( %client.isAIControlled() )
   {
      if ( %leastTeam & 1 )
      {
	 %client.skin = addTaggedString( "basebot" );
	 setTargetSkin( %client.target, 'basebot' );
      }
      else
      {
	 %client.skin = addTaggedString( "basebbot" );
	 setTargetSkin( %client.target, 'basebbot' );
      }
   }
   else
      setTargetSkin( %client.target, %game.getTeamSkin(%client.team) );
      //setTargetSkin( %client.target, %client.skin );

   // might as well standardize the messages
   //messageAllExcept( %client, -1, 'MsgClientJoinTeam', '\c1%1 joined %2.', %client.name, $teamName[%leastTeam], %client, %leastTeam );
   //messageClient( %client, 'MsgClientJoinTeam', '\c1You joined the %2 team.', $client.name, $teamName[%client.team], %client, %client.team );
   messageAllExcept( %client, -1, 'MsgClientJoinTeam', '\c1%1 joined %2.', %client.name, %game.getTeamName(%client.team), %client, %client.team );
   messageClient( %client, 'MsgClientJoinTeam', '\c1You joined the %2 team.', %client.name, %game.getTeamName(%client.team), %client, %client.team );

   updateCanListenState( %client );

   logEcho(%client.nameBase@" (cl "@%client@") joined team "@%client.team);
}

function DefaultGame::getTeamSkin(%game, %team)
{
    //error("DefaultGame::getTeamSkin");
    %skin = $teamSkin[%team];
    //error("%skin = " SPC getTaggedString(%skin));
    return %skin;
}

function DefaultGame::getTeamName(%game, %team)
{
    //error("DefaultGame::getTeamName");
    %name = $teamName[%team];
    //error("name = " SPC getTaggedString(%name));
    return %name;
}

function DefaultGame::clientJoinTeam( %game, %client, %team, %respawn )
{
	// for multi-team games played with a single team 
	if (Game.numTeams == 1) {
		%game.assignClientTeam( %client );
		// Spawn the player:
		%game.spawnPlayer( %client, %respawn );
		return;
	}

//error("DefaultGame::clientJoinTeam");
   if ( %team < 1 || %team > %game.numTeams )
      return;

   if( %respawn $= "" )
      %respawn = 1;

   %client.team = %team;
   %client.lastTeam = %team;
   setTargetSkin( %client.target, %game.getTeamSkin(%team) );
   setTargetSensorGroup( %client.target, %team );
   %client.setSensorGroup( %team );

   // Spawn the player:
   %game.spawnPlayer( %client, %respawn );

   messageAllExcept( %client, -1, 'MsgClientJoinTeam', '\c1%1 joined %2.', %client.name, %game.getTeamName(%team), %client, %team );
   messageClient( %client, 'MsgClientJoinTeam', '\c1You joined the %2 team.', $client.name, %game.getTeamName(%client.team), %client, %client.team );

   updateCanListenState( %client );

   logEcho(%client.nameBase@" (cl "@%client@") joined team "@%client.team);
	if ($Host::Prison::Enabled == true) {
		if (%client.isJailed)
			// If player should manage to get out of jail, re-spawn and re-start sentence time
			jailPlayer(%client,false,mAbs(%cl.jailTime));
	}
}

function DefaultGame::AIHasJoined(%game, %client)
{
   //defined to prevent console spam
}

function DefaultGame::AIChangeTeam(%game, %client, %newTeam)
{
   //make sure we're trying to drop an AI
   if (!isObject(%client) || !%client.isAIControlled())
      return;

   //clear the ai from any objectives, etc...
	AIUnassignClient(%client);
   %client.stop();
	%client.clearTasks();
   %client.clearStep();
   %client.lastDamageClient = -1;
   %client.lastDamageTurret = -1;
   %client.shouldEngage = -1;
   %client.setEngageTarget(-1);
   %client.setTargetObject(-1);
	%client.pilotVehicle = false;
   %client.defaultTasksAdded = false;

   //kill the player, which should cause the Game object to perform whatever cleanup is required.
   if (isObject(%client.player))
      %client.player.scriptKill(0);

   //clean up the team rank array
   %game.removeFromTeamRankArray(%client);

   //assign the new team
   %client.team = %newTeam;
   if (%newTeam < 0)
      Game.assignClientTeam(%client);
   else
   {
      if ( %client.team & 1 )
      {
	 %client.skin = addTaggedString( "basebot" );
	 setTargetSkin( %client.target, 'basebot' );
      }
      else
      {
	 %client.skin = addTaggedString( "basebbot" );
	 setTargetSkin( %client.target, 'basebbot' );
      }
   }

   messageAllExcept( %client, -1, 'MsgClientJoinTeam', '\c1bot %1 has switched to team %2.', %client.name, %game.getTeamName(%client.team), %client, %client.team );
}

function DefaultGame::clientChangeTeam(%game, %client, %team, %fromObs)
{
//error("DefaultGame::clientChangeTeam");
   //first, remove the client from the team rank array
   //the player will be added to the new team array as soon as he respawns...
   %game.removeFromTeamRankArray(%client);

   %pl = %client.player;
   if(isObject(%pl))
   {
      if(%pl.isMounted())
	 %pl.getDataBlock().doDismount(%pl);
      %pl.scriptKill(0);
   }

   // reset the client's targets and tasks only
   clientResetTargets(%client, true);

   // give this client a new handle to disassociate ownership of deployed objects
   if( %team $= "" && (%team > 0 && %team <= %game.numTeams))
   {
      if( %client.team == 1 )
	 %client.team = 2;
      else
	 %client.team = 1;
   }
   else
      %client.team = %team;

// Shouldn't be needed - serverCmds are proofed (hopefully)
// As it is, it just prevents admins from changing non-admins to a non-valid team
//	if (!(%client.isAdmin || %client.isSuperAdmin)) {
//		if (%team > %game.numTeams || %team $= "")
//			%team = 1;
//	}

   // Set the client's skin:
   if (!%client.isAIControlled())
      setTargetSkin( %client.target, %game.getTeamSkin(%client.team) );
   setTargetSensorGroup( %client.target, %client.team );
   %client.setSensorGroup( %client.team );

   // Spawn the player:
   %client.lastSpawnPoint = %game.pickPlayerSpawn( %client );

   %game.createPlayer( %client, %client.lastSpawnPoint, $MatchStarted );

   if($MatchStarted)
      %client.setControlObject(%client.player);
   else
   {
      %client.camera.getDataBlock().setMode(%client.camera, "pre-game", %client.player);
      %client.setControlObject(%client.camera);
   }

   // call the onEvent for this game type
   %game.onClientEnterObserverMode(%client);  //Bounty uses this to remove this client from others' hit lists

   if(%fromObs $= "" || !%fromObs)
   {
      messageAllExcept( %client, -1, 'MsgClientJoinTeam', '\c1%1 switched to team %2.', %client.name, %game.getTeamName(%client.team), %client, %client.team );
      messageClient( %client, 'MsgClientJoinTeam', '\c1You switched to team %2.', $client.name, %game.getTeamName(%client.team), %client, %client.team );
   }
   else
   {
      messageAllExcept( %client, -1, 'MsgClientJoinTeam', '\c1%1 joined team %2.', %client.name, %game.getTeamName(%client.team), %client, %team );
      messageClient( %client, 'MsgClientJoinTeam', '\c1You joined team %2.', $client.name, %game.getTeamName(%client.team), %client, %client.team );
   }

   updateCanListenState( %client );

   // MES - switch objective hud lines when client switches teams
   messageClient(%client, 'MsgCheckTeamLines', "", %client.team);
   logEcho(%client.nameBase@" (cl "@%client@") switched to team "@%client.team);
}

//  missioncleanup and missiongroup are checked prior to entering game code
function DefaultGame::missionLoadDone(%game)
{
   //  walks through the mission group and sets the power stuff up
   //   - groups get initialized with power count 0 then iterated to
   //     increment powercount if an object within is powered
   //   - powers objects up/down
   //MissionGroup.objectiveInit();
   MissionGroup.clearPower();
   MissionGroup.powerInit(0);

   %game.initGameVars();  //set up scoring variables and other game specific globals

   // make team0 visible/friendly to all
   setSensorGroupAlwaysVisMask(0, 0xffffffff);
   setSensorGroupFriendlyMask(0, 0xffffffff);

   // update colors:
   // - enemy teams are red
   // - same team is green
   // - team 0 is white
   for(%i = 0; %i < 32; %i++)
   {
      %team = (1 << %i);
      setSensorGroupColor(%i, %team, "0 255 0 255");
      setSensorGroupColor(%i, ~%team, "255 0 0 255");
      setSensorGroupColor(%i, 1, "255 255 255 255");

      // setup the team targets (alwyas friendly and visible to same team)
      setTargetAlwaysVisMask(%i, %team);
      setTargetFriendlyMask(%i, %team);
   }

   //set up the teams
   %game.setUpTeams();

   //clear out the team rank array...
   for (%i = 0; %i < 32; %i++)
      $TeamRank[%i, count] = "";

   // objectiveInit has to take place after setupTeams -- objective HUD relies on flags
   // having their team set
   MissionGroup.objectiveInit();

   //initialize the AI system
   %game.aiInit();

   //need to reset the teams if we switch from say, CTF to Bounty...
   // assign the bots team
   if ($currentMissionType !$= $previousMissionType)
   {
      $previousMissionType = $currentMissionType;
      for(%i = 0; %i < ClientGroup.getCount(); %i++)
      {
	 %cl = ClientGroup.getObject(%i);
	 if (%cl.isAIControlled())
	    %game.assignClientTeam(%cl);
      }
   }

   //Save off respawn or Siege Team switch information...
   if(%game.class !$= "SiegeGame")
      MissionGroup.setupPositionMarkers(true);
   echo("Default game mission load done.");
}

function DefaultGame::onClientLeaveGame(%game, %client)
{
   // if there is a player attached to this client, kill it
   if( isObject(%client.player))
      %client.player.scriptKill(0);

   //cancel a scheduled call...
   cancel(%client.respawnTimer);
   %client.respawnTimer = "";

   //remove them from the team rank arrays
   %game.removeFromTeamRankArray(%client);
   logEcho(%client.nameBase@" (cl "@%client@") dropped");
}

function DefaultGame::clientMissionDropReady(%game, %client)
{
   //synchronize the clock HUD
   messageClient(%client, 'MsgSystemClock', "", 0, 0);

   %game.sendClientTeamList( %client );
   %game.setupClientHuds( %client );

   if($CurrentMissionType $= "SinglePlayer")
   {
      //CommandToClient( %client, 'setPlayContent');
      return;
   }

   %observer = false;
   if( !$Host::TournamentMode )
   {
      if( %client.camera.mode $= "observerFly" || %client.camera.mode $= "justJoined")
      {
	 %observer = true;
	 %client.observerStartTime = getSimTime();
	 commandToClient(%client, 'setHudMode', 'Observer');
	 %client.setControlObject( %client.camera );
	 //displayObserverHud( %client, 0 );
	 updateObserverFlyHud(%client);
      }

      if( !%observer )
      {
	 if(!$MatchStarted && !$CountdownStarted) // server has not started anything yet
	 {
	    %client.setControlObject( %client.camera );
	    commandToClient(%client, 'setHudMode', 'Observer');
	 }
	 else if(!$MatchStarted && $CountdownStarted) // server has started the countdown
	 {
	    commandToClient(%client, 'setHudMode', 'Observer');
	    %client.setControlObject( %client.camera );
	 }
	 else
	 {
	    commandToClient(%client, 'setHudMode', 'Standard'); // the game has already started
	    %client.setControlObject( %client.player );
	 }
      }
   }
   else
   {
      // set all players into obs mode. setting the control object will handle further procedures...
      %client.camera.getDataBlock().setMode( %client.camera, "ObserverFly" );
      commandToClient(%client, 'setHudMode', 'Observer');
      %client.setControlObject( %client.camera );
      messageAll( 'MsgClientJoinTeam', "",%client.name, $teamName[0], %client, 0 );
      %client.team = 0;

      if( !$MatchStarted && !$CountdownStarted)
      {
	 if($TeamDamage)
	    %damMess = "ENABLED";
	 else
	    %damMess = "DISABLED";

	 if(%game.numTeams > 1)
	    BottomPrint(%client, "Server is Running in Tournament Mode.\nPick a Team\nTeam Damage is " @ %damMess, 0, 3 );
      }
      else
      {
	 BottomPrint( %client, "\nServer is Running in Tournament Mode", 0, 3 );
      }
   }

   //make sure the objective HUD indicates your team on top and in green...
   if (%client.team > 0)
      messageClient(%client, 'MsgCheckTeamLines', "", %client.team);

   // were ready to go.
   %client.matchStartReady = true;
   echo("Client" SPC %client SPC "is ready.");

   if ( isDemo() )
   {
      if ( %client.demoJustJoined )
      {
	 %client.demoJustJoined = false;
	 centerPrint( %client, "Welcome to the Tribes 2 Demo." NL "You have been assigned the name \"" @ %client.nameBase @ "\"." NL "Press FIRE to join the game.", 0, 3 );
      }
   }
}

function DefaultGame::sendClientTeamList(%game, %client)
{
   // Send the client the current team list:
   %teamCount = %game.numTeams;
   for ( %i = 0; %i < %teamCount; %i++ )
   {
      if ( %i > 0 )
	 %teamList = %teamList @ "\n";

      %teamList = %teamList @ detag( getTaggedString( %game.getTeamName(%i + 1) ) );
   }
   messageClient( %client, 'MsgTeamList', "", %teamCount, %teamList );
}

function DefaultGame::setupClientHuds(%game, %client)
{
   // tell the client to setup the huds...
   for(%i =0; %i<$WeaponsHudCount; %i++)
      %client.setWeaponsHudBitmap(%i, $WeaponsHudData[%i, itemDataName], $WeaponsHudData[%i, bitmapName]);
   for(%i =0; %i<$InventoryHudCount; %i++)
   {
      if ( $InventoryHudData[%i, slot] != 0 )
	 %client.setInventoryHudBitmap($InventoryHudData[%i, slot], $InventoryHudData[%i, itemDataName], $InventoryHudData[%i, bitmapName]);
   }
   %client.setInventoryHudBitmap( 0, "", "gui/hud_handgren" );

   %client.setWeaponsHudBackGroundBmp("gui/hud_new_panel");
   %client.setWeaponsHudHighLightBmp("gui/hud_new_weaponselect");
   %client.setWeaponsHudInfiniteAmmoBmp("gui/hud_infinity");
   %client.setInventoryHudBackGroundBmp("gui/hud_new_panel");

   // tell the client if we are protecting statics (so no health bar will be displayed)
// We have deployable station inventories that can be destroyed, and thus we need the health bars
//   commandToClient(%client, 'protectingStaticObjects', %game.allowsProtectedStatics());
   commandToClient(%client, 'setPowerAudioProfiles', sPowerUp.getId(), sPowerDown.getId());
}

function DefaultGame::testDrop( %game, %client )
{
   %game.clientJoinTeam( %client, 1, false );
   %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
   %client.setControlObject( %client.camera );
   CommandToClient( %client, 'setPlayContent' );
}

function DefaultGame::onClientEnterObserverMode( %game, %client )
{
   // Default game doesn't care...
}

// from 'item.cs'
function DefaultGame::playerTouchFlag(%game, %player, %flag)
{
   messageAll('MsgPlayerTouchFlag', 'Player %1 touched flag %2', %player, %flag);
}

// from 'item.cs'
function DefaultGame::playerDroppedFlag(%game, %player, %flag)
{
   messageAll('MsgPlayerDroppedFlag', 'Player %1 dropped flag %2', %player, %flag);
}

// from 'staticShape.cs'
function DefaultGame::flagStandCollision(%game, %dataBlock, %obj, %colObj)
{
   // for retreiveGame
}

function DefaultGame::notifyMineDeployed(%game, %mine)
{
   //do nothign in the default game...
}

// from 'staticshape.cs'
function DefaultGame::findProjector(%game, %flipflop)
{
   // search the flipflop's folder for a holo projector
   // if one exists, associate it with the flipflop
   %flipflop.projector = 0;
   %folder = %flipflop.getGroup();
   for(%i = 0; %i < %folder.getCount(); %i++)
   {
      %proj = %folder.getObject(%i);
      if(%proj.getDatablock().getName() $= "LogoProjector")
      {
	 %flipflop.projector = %proj;
	 %flipflop.projector.holo = 0;
	 break;
      }
   }
}

//******************************************************************************
//*   DefaultGame Trigger  -  Functions                                        *
//******************************************************************************

/// -Trigger- //////////////////////////////////////////////////////////////////
//Function -- onEnterTrigger (%game, %name, %data, %obj, %colObj)
//                %game = Current game type object
//                %name = Trigger name - defined when trigger is created
//                %data = Trigger Data Block
//                %obj = Trigger Object
//                %colObj = Object that collided with the trigger
//Decription -- Called when trigger has been triggered
////////////////////////////////////////////////////////////////////////////////
// from 'trigger.cs'
function DefaultGame::onEnterTrigger(%game, %triggerName, %data, %obj, %colobj)
{
   //Do Nothing
}

/// -Trigger- //////////////////////////////////////////////////////////////////
//Function -- onLeaveTrigger (%game, %name, %data, %obj, %colObj)
//                %game = Current game type object
//                %name = Trigger name - defined when trigger is created
//                %data = Trigger Data Block
//                %obj = Trigger Object
//                %colObj = Object that collided with the trigger
//Decription -- Called when trigger has been untriggered
////////////////////////////////////////////////////////////////////////////////
// from 'trigger.cs'
function DefaultGame::onLeaveTrigger(%game, %triggerName, %data, %obj, %colobj)
{
   //Do Nothing
}

/// -Trigger- //////////////////////////////////////////////////////////////////
//Function -- onTickTrigger(%game, %name, %data, %obj)
//                %game = Current game type object
//                %name = Trigger name - defined when trigger is created
//                %data = Trigger Data Block
//                %obj = Trigger Object
//Decription -- Called every tick if triggered
////////////////////////////////////////////////////////////////////////////////
// from 'trigger.cs'
function DefaultGame::onTickTrigger(%game, %triggerName, %data, %obj)
{
   //Do Nothing
}


function DefaultGame::setUpTeams(%game)
{
   %group = nameToID("MissionGroup/Teams");
   if(%group == -1)
      return;

   // create a team0 if it does not exist
   %team = nameToID("MissionGroup/Teams/team0");
   if(%team == -1)
   {
      %team = new SimGroup("team0");
      %group.add(%team);
   }

   // 'team0' is not counted as a team here
   %game.numTeams = 0;
   while(%team != -1)
   {
      // create drop set and add all spawnsphere objects into it
      %dropSet = new SimSet("TeamDrops" @ %game.numTeams);
      MissionCleanup.add(%dropSet);

      %spawns = nameToID("MissionGroup/Teams/team" @ %game.numTeams @ "/SpawnSpheres");
      if(%spawns != -1)
      {
	 %count = %spawns.getCount();
	 for(%i = 0; %i < %count; %i++)
	    %dropSet.add(%spawns.getObject(%i));
      }

      // set the 'team' field for all the objects in this team
      %team.setTeam(%game.numTeams);

      clearVehicleCount(%team+1);
      // get next group
      %team = nameToID("MissionGroup/Teams/team" @ %game.numTeams + 1);
      if (%team != -1)
	 %game.numTeams++;
   }

   // set the number of sensor groups (including team0) that are processed
   setSensorGroupCount(%game.numTeams + 1);
}

function SimGroup::setTeam(%this, %team)
{
   for (%i = 0; %i < %this.getCount(); %i++)
   {
      %obj = %this.getObject(%i);
      switch$ (%obj.getClassName())
      {
	 case SpawnSphere     :
	    if($MatchStarted)
	    {
	       // find out what team the spawnsphere used to belong to
	       %found = false;
	       for(%l = 1; %l <= Game.numTeams; %l++)
	       {
		  %drops = nameToId("MissionCleanup/TeamDrops" @ %l);
		  for(%j = 0; %j < %drops.getCount(); %j++)
		  {
		     %current = %drops.getObject(%j);
		     if(%current == %obj)
			%found = %l;
		  }
	       }
	       if(%team != %found)
		  Game.claimSpawn(%obj, %team, %found);
	       else
		  error("spawn "@%obj@" is already on team "@%team@"!");
	    }
	    else
	       Game.claimSpawn(%obj, %team, "");
	 case SimGroup        :  %obj.setTeam(%team);
	 default              :  %obj.team = %team;
      }

      if(%obj.getType() & $TypeMasks::GameBaseObjectType)
      {
         // eeck.. please go away when scripts get cleaned...
         // -----------------------------------------------------------------------------
         // z0dd - ZOD, 5/8/02. Part of re-write of Vehicle
         // station creation. Do not need this code anymore.
         //if(%obj.getDataBlock().getName() $= "StationVehiclePad")
         //{
         //   %team = %obj.team;
         //   %obj = %obj.station;
         //   %obj.team = %team;
            //%obj.teleporter.team = %team;
         //}
         %target = %obj.getTarget();
         if(%target != -1)
            setTargetSensorGroup(%target, %team);
      }
   }
}

function DefaultGame::claimSpawn(%game, %obj, %newTeam, %oldTeam)
{
   if(%newTeam == %oldTeam)
      return;

   %newSpawnGroup = nameToId("MissionCleanup/TeamDrops" @ %newTeam);
   if(%oldTeam !$= "")
   {
      %oldSpawnGroup = nameToId("MissionCleanup/TeamDrops" @ %oldTeam);
      %oldSpawnGroup.remove(%obj);
   }
   %newSpawnGroup.add(%obj);
}

// recursive function to assign teams to all mission objects

function SimGroup::swapTeams(%this)
{
   // used in Siege only
   Game.groupSwapTeams(%this);
}

function ShapeBase::swapTeams(%this)
{
   // used in Siege only
   Game.objectSwapTeams(%this);
}

function GameBase::swapTeams(%this)
{
   // used in Siege only
   Game.objectSwapTeams(%this);
}

function TSStatic::swapTeams(%this)
{
   // used in Siege only
   // do nothing
}

function InteriorInstance::swapTeams(%this)
{
   // used in Siege only
   // do nothing -- interiors don't switch teams
}

function SimGroup::swapVehiclePads(%this)
{
   // used in Siege only
   Game.groupSwapVehiclePads(%this);
}

function ShapeBase::swapVehiclePads(%this)
{
   // used in Siege only
   Game.objectSwapVehiclePads(%this);
}

function GameBase::swapVehiclePads(%this)
{
   // used in Siege only
   // do nothing -- only searching for vehicle pads
}

function InteriorInstance::swapVehiclePads(%this)
{
   // used in Siege only
   // do nothing -- only searching for vehicle pads
}

function SimSet::swapVehiclePads(%this)
{
   // used in Siege only
   // do nothing -- only searching for vehicle pads
}

function PhysicalZone::swapVehiclePads(%this)
{
   // used in Siege only
   // do nothing -- only searching for vehicle pads
}

function SimGroup::objectRestore(%this)
{
   // used in Siege only
   Game.groupObjectRestore(%this);
}

function ShapeBase::objectRestore(%object)
{
   // only used for Siege
   Game.shapeObjectRestore(%object);
}

function Turret::objectRestore(%object)
{
   // only used for Siege
   Game.shapeObjectRestore(%object);
}

function AIObjective::objectRestore(%object)
{
   // only used for Siege
   // don't do anything for AI Objectives
}

function DefaultGame::checkObjectives(%game)
{
   //any special objectives that can be met by gametype
   //none for default game
}

//---------------------------------------------------

function DefaultGame::checkTimeLimit(%game, %forced)
{
   // Don't add extra checks:
   if ( %forced )
      cancel( %game.timeCheck );

   // if there is no time limit, check back in a minute to see if it's been set
   if(($Host::TimeLimit $= "") || $Host::TimeLimit == 0)
   {
      %game.timeCheck = %game.schedule(20000, "checkTimeLimit");
      return;
   }

   %curTimeLeftMS = ($Host::TimeLimit * 60 * 1000) + $missionStartTime - getSimTime();

   if (%curTimeLeftMS <= 0)
   {
      // time's up, put down your pencils
      %game.timeLimitReached();
   }
   else
   {
      if(%curTimeLeftMS >= 20000)
	 %game.timeCheck = %game.schedule(20000, "checkTimeLimit");
      else
	 %game.timeCheck = %game.schedule(%curTimeLeftMS + 1, "checkTimeLimit");

      //now synchronize everyone's clock
      messageAll('MsgSystemClock', "", $Host::TimeLimit, %curTimeLeftMS);
   }
}

function listplayers()
{
   for(%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      %status = "";
		if(%cl.isAiControlled())
			%status = "Bot ";
      if(%cl.isSmurf)
	 %status = "Alias ";
      if(%cl.isAdmin)
	 %status = %status @ "Admin ";
      if(%cl.isSuperAdmin)
	 %status = %status @ "SuperAdmin ";
      if(%status $= "")
	 %status = "<normal>";
      echo("client: " @ %cl @ " player: " @ %cl.player @ " name: " @ %cl.nameBase @ " team: " @ %cl.team @ " status: " @ %status);
   }
}

function DefaultGame::clearTeamRankArray(%game, %team)
{
   %count = $TeamRank[%team, count];
   for (%i = 0; %i < %count; %i++)
      $TeamRank[%team, %i] = "";
   $TeamRank[%team, count] = 0;
}

function DefaultGame::populateTeamRankArray(%game, %client)
{
   //this function should be called *after* the client has been added to a team...
   if (%client <= 0 || %client.team <= 0)
      return;

   //find the team
   if (%game.numTeams == 1)
      %team = 0;
   else
      %team = %client.team;

   //find the number of teammates already ranked...
   %count = $TeamRank[%team, count];
   if (%count $= "")
   {
      $TeamRank[%team, count] = 0;
      %count = 0;
   }

   //make sure we're not already in the array
   for (%i = 0; %i < %count; %i++)
   {
      if ($TeamRank[%team, %i] == %client)
	 return;
   }

   //add the client in at the bottom of the list, and increment the count
   $TeamRank[%team, %count] = %client;
   $TeamRank[%team, count] = $TeamRank[%team, count] + 1;

   //now recalculate the team rank for this player
   %game.recalcTeamRanks(%client);
}

function DefaultGame::removeFromTeamRankArray(%game, %client)
{
   //note, this should be called *before* the client actually switches teams or drops...
   if (%client <= 0 || %client.team <= 0)
      return;

   //find the correct team
   if (%game.numTeams == 1)
      %team = 0;
   else
      %team = %client.team;

   //now search throught the team rank array, looking for this client
   %count = $TeamRank[%team, count];
   for (%i = 0; %i < %count; %i++)
   {
      if ($TeamRank[%team, %i] == %client)
      {
	 //we've found the client in the array, now loop through, and move everyone else up a rank
	 for (%j = %i + 1; %j < %count; %j++)
	 {
	    %cl = $TeamRank[%team, %j];
	    $TeamRank[%team, %j - 1] = %cl;
	    messageClient(%cl, 'MsgYourRankIs', "", %j);
	 }
	 $TeamRank[%team, %count - 1] = "";

	 //now decrement the team rank array count, and break
	 $TeamRank[%team, count] = $TeamRank[%team, count] - 1;
	 break;
      }
   }
}

function DefaultGame::recalcTeamRanks(%game, %client)
{
   if (%client <= 0 || %client.team <= 0)
      return;

   // this is a little confusing -- someone's actual numerical rank is always
   // one number higher than his index in the $TeamRank array
   // (e.g. person ranked 1st has index of 0)

   // TINMAN:  I'm going to remove the %client.teamRank field - the index in the
   // $TeamRank array already contains their rank - safer to search the array than
   // to maintiain the information in a separate variable...

   //find the team, the client in the team array
   if (%game.numTeams == 1)
      %team = 0;
   else
      %team = %client.team;

   %count = $TeamRank[%team, count];
   %index = -1;
   for (%i = 0; %i < %count; %i++)
   {
      if ($TeamRank[%team, %i] == %client)
      {
	 %index = %i;
	 break;
      }
   }

   //if they weren't found in the array, return
   if (%index < 0)
      return;

   //make sure far down the array as they should be...
   %tempIndex = %index;
   %swapped = false;
   while (true)
   {
      if (%tempIndex <= 0)
	 break;

      %tempIndex--;
      %tempClient = $TeamRank[%team, %tempIndex];

      //see if we should swap the two
      if (%client.score > %tempClient.score)
      {
	 %swapped = true;
	 %index = %tempIndex;
	 $TeamRank[%team, %tempIndex] = %client;
	 $TeamRank[%team, %tempIndex + 1] = %tempClient;
	 messageClient(%tempClient, 'MsgYourRankIs', "", %tempIndex + 2);
      }
   }

   //if we've swapped up at least once, we obviously won't need to swap down as well...
   if (%swapped)
   {
      messageClient(%client, 'MsgYourRankIs', "", %index + 1);
      return;
   }

   //since we didnt' swap up, see if we need to swap down...
   %tempIndex = %index;
   %swapped = false;
   while (true)
   {
      if (%tempIndex >= %count - 1)
	 break;

      %tempIndex++;
      %tempClient = $TeamRank[%team, %tempIndex];

      //see if we should swap the two
      if (%client.score < %tempClient.score)
      {
	 %swapped = true;
	 %index = %tempIndex;
	 $TeamRank[%team, %tempIndex] = %client;
	 $TeamRank[%team, %tempIndex - 1] = %tempClient;
	 messageClient(%tempClient, 'MsgYourRankIs', "", %tempIndex);
      }
   }

   //send the message (regardless of whether a swap happened or not)
   messageClient(%client, 'MsgYourRankIs', "", %index + 1);
}

function DefaultGame::recalcScore(%game, %cl)
{
   %game.recalcTeamRanks(%cl);
}

function DefaultGame::testKill(%game, %victimID, %killerID)
{
   return ((%killerID !=0) && (%victimID.team != %killerID.team));
}

function DefaultGame::testSuicide(%game, %victimID, %killerID, %damageType)
{
   return ((%victimID == %killerID) || (%damageType == $DamageType::Ground) || (%damageType == $DamageType::Suicide));
}

function DefaultGame::testTeamKill(%game, %victimID, %killerID)
{
   return (%killerID.team == %victimID.team);
}

function DefaultGame::testTurretKill(%game, %implement)
{
   if(%implement == 0)
      return false;
   else
      return (%implement.getClassName() $= "Turret");
}

// function DefaultGame::awardScoreFlagCap(%game, %cl)
// {
//    %cl.flagCaps++;
//    $TeamScore[%cl.team] += %game.SCORE_PER_TEAM_FLAG_CAP;
//    messageAll('MsgCTFTeamScore', "", %cl.team, $TeamScore[%cl.team]);
//
//    if (%game.SCORE_PER_PLYR_FLAG_CAP > 1)
//      %plural = "s";
//    else
//      %plural = "";
//
//    if (%game.SCORE_PER_PLYR_FLAG_CAP != 0)
//         messageClient(%cl, 'scoreFlaCapMsg', 'You received %1 point%2 for capturing the flag.', %game.SCORE_PER_PLYR_FLAG_CAP, %plural);
//    %game.recalcScore(%cl);
// }


function DefaultGame::testOOBDeath(%game, %damageType)
{
   return (%damageType == $DamageType::OutOfBounds);
}

function DefaultGame::awardScoreTurretKill(%game, %victimID, %implement)
{
   if ((%killer = %implement.getControllingClient()) != 0) //award whoever might be controlling the turret
   {
      if (%killer == %victimID)
	   %game.awardScoreSuicide(%victimID);
      else if (%killer.team == %victimID.team) //player controlling a turret killed a teammate
      {
	   %killer.teamKills++;
	%game.awardScoreTurretTeamKill(%victimID, %killer);
	%game.awardScoreDeath(%victimID);
      }
      else
      {
	 %killer.turretKills++;
	 %game.recalcScore(%killer);
	 %game.awardScoreDeath(%victimID);
      }
   }
   else if ((%killer = %implement.getOwner()) != 0) //if it isn't controlled, award score to whoever deployed it
   {
       if (%killer.team == %victimID.team)
       {
	    %game.awardScoreDeath(%victimID);
       }
       else
       {
	  %killer.turretKills++;
	 %game.recalcScore(%killer);
	 %game.awardScoreDeath(%victimID);
      }
   }
   //default is, no one was controlling it, no one owned it.  No score given.
}

function DefaultGame::awardScoreDeath(%game, %victimID)
{
   %victimID.deaths++;
   if ( %game.SCORE_PER_DEATH != 0 )
   {
//       %plural = (abs(%game.SCORE_PER_DEATH) != 1 ? "s" : "");
//       messageClient(%victimID, 'MsgScoreDeath', '\c0You have been penalized %1 point%2 for dying.', abs(%game.SCORE_PER_DEATH), %plural);
      %game.recalcScore(%victimID);
   }
}

function DefaultGame::awardScoreKill(%game, %killerID)
{
   %killerID.kills++;
   %game.recalcScore(%killerID);
}

function DefaultGame::awardScoreSuicide(%game, %victimID)
{
   %victimID.suicides++;
//    if (%game.SCORE_PER_SUICIDE != 0)
//       messageClient(%victimID, 'MsgScoreSuicide', '\c0You have been penalized for killing yourself.');
   %game.recalcScore(%victimID);
}

function DefaultGame::awardScoreTeamkill(%game, %victimID, %killerID)
{
   %killerID.teamKills++;
   if (%game.SCORE_PER_TEAMKILL != 0)
      messageClient(%killerID, 'MsgScoreTeamkill', '\c0You have been penalized for killing teammate %1.', %victimID.name);
   %game.recalcScore(%killerID);
}

function DefaultGame::awardScoreTurretTeamKill(%game, %victimID, %killerID)
{
   %killerID.teamKills++;
   if (%game.SCORE_PER_TEAMKILL != 0)
      messageClient(%killerID, 'MsgScoreTeamkill', '\c0You have been penalized for killing your teammate %1, with a turret.', %victimID.name);
   %game.recalcScore(%killerID);
}


function DefaultGame::objectRepaired(%game, %obj, %objName)
{
   %item = %obj.getDataBlock().getName();
   //echo("Item repaired is a " @ %item);
   switch$ (%item)
   {
      case generatorLarge :
	 %game.genOnRepaired(%obj, %objName);
      case stationInventory :
	 %game.stationOnRepaired(%obj, %objName);
      case sensorMediumPulse :
	 %game.sensorOnRepaired(%obj, %objName);
      case sensorLargePulse :
	 %game.sensorOnRepaired(%obj, %objName);
      case turretBaseLarge :
	 %game.turretOnRepaired(%obj, %objName);
      case stationVehicle : %game.vStationOnRepaired(%obj, %objName);
      default:  //unused by current gametypes.  Add more checks here if desired
   }
}

function DefaultGame::allowsProtectedStatics(%game)
{
   return false;
}

// jff: why is game object doing this?
//Return a simple string with no extras
function DefaultGame::cleanWord(%game, %this)
{
   %length = strlen(%this);
   for(%i = 0; %i < %length; %i++)
   {
      %char = getSubStr(%this, %i, 1);
      if(%char $= "_")
      {
	 %next =  getSubStr(%this, (%i+1), 1);
	 if(%next $= "_")
	 {
	    %char = "'";   //apostrophe (2 chars)
	    %i++;
	 }
	 else
	    %char = " ";   //space
      }
      %clean = (%clean @ %char);
   }
}

function DefaultGame::stationOnEnterTrigger(%game, %data, %obj, %colObj)
{
   return true;
}

function DefaultGame::WeaponOnUse(%game, %data, %obj)
{
   return true;
}

function DefaultGame::HandInvOnUse(%game, %data, %obj)
{
   return true;
}

function DefaultGame::WeaponOnInventory(%game, %this, %obj, %amount)
{
   return true;
}

function DefaultGame::ObserverOnTrigger(%game, %data, %obj, %trigger, %state)
{
   return true;
}

// jff: why is the game being notified that a weapon is being thrown? hot potato gametype?
function DefaultGame::ShapeThrowWeapon(%game, %this)
{
   return true;
}

function DefaultGame::leaveMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;

   %player.client.outOfBounds = true;
   messageClient(%player.client, 'LeaveMissionArea', '\c1You left the mission area.~wfx/misc/warning_beep.wav');
}

function DefaultGame::enterMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;

   %player.client.outOfBounds = false;
   messageClient(%player.client, 'EnterMissionArea', '\c1You are back in the mission area.');
}

//------------------------------------------------------------------------------
// AI stubs:
//------------------------------------------------------------------------------

function DefaultGame::onAIDamaged(%game, %clVictim, %clAttacker, %damageType, %sourceObject)
{
}

function DefaultGame::onAIFriendlyFire(%game, %clVictim, %clAttacker, %damageType, %sourceObject)
{
}

function DefaultGame::onAIKilled(%game, %clVictim, %clKiller, %damageType, %implement)
{
   //unassign the client from any objectives
   AIUnassignClient(%clVictim);

   //break the link, if this ai is controlled
   aiReleaseHumanControl(%clVictim.controlByHuman, %clVictim);

   //and schedule the respawn
   %clVictim.respawnThread = schedule(5000, %clVictim, "onAIRespawn", %clVictim);
}

function DefaultGame::onAIKilledClient(%game, %clVictim, %clAttacker, %damageType, %implement)
{
   %clAttacker.setVictim(%clVictim, %clVictim.player);
}

//------------------------------------------------------------------------------
// Voting stuff:
//------------------------------------------------------------------------------
function DefaultGame::sendGamePlayerPopupMenu( %game, %client, %targetClient, %key )
{
   if( !%targetClient.matchStartReady )
      return;

   %isAdmin = ( %client.isAdmin || %client.isSuperAdmin );

   %isTargetSelf = ( %client == %targetClient );
   %isTargetAdmin = ( %targetClient.isAdmin || %targetClient.isSuperAdmin );
   %isTargetBot = %targetClient.isAIControlled();
   %isTargetObserver = ( %targetClient.team == 0 );
   %outrankTarget = false;
   if ( %client.isSuperAdmin )
      %outrankTarget = !%targetClient.isSuperAdmin;
   else if ( %client.isAdmin )
      %outrankTarget = !%targetClient.isAdmin;

   if( %client.isSuperAdmin && %targetClient.guid != 0 && !isDemo() )
   {
      messageClient( %client, 'MsgPlayerPopupItem', "", %key, "addAdmin", "", 'Add to Server Admin List', 10);
      messageClient( %client, 'MsgPlayerPopupItem', "", %key, "addSuperAdmin", "", 'Add to Server SuperAdmin List', 11);
   }

   //mute options
   if ( !%isTargetSelf )
   {
      if ( %client.muted[%targetClient] )
	 messageClient( %client, 'MsgPlayerPopupItem', "", %key, "MutePlayer", "", 'Unmute Text Chat', 1);
      else
	 messageClient( %client, 'MsgPlayerPopupItem', "", %key, "MutePlayer", "", 'Mute Text Chat', 1);

      if ( !%isTargetBot && %client.canListenTo( %targetClient ) )
      {
	 if ( %client.getListenState( %targetClient ) )
	    messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ListenPlayer", "", 'Disable Voice Com', 9 );
	 else
	    messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ListenPlayer", "", 'Enable Voice Com', 9 );
      }
   }

   if( !%client.canVote && !%isAdmin )
      return;

   // regular vote options on players
   if ( %game.scheduleVote $= "" && !%isAdmin && !%isTargetAdmin )
   {
      if ( $Host::allowAdminPlayerVotes && !%isTargetBot && !isDemo() )
	 messageClient( %client, 'MsgPlayerPopupItem', "", %key, "AdminPlayer", "", 'Vote to Make Admin', 2 );

      if ( !%isTargetSelf )
      {
	 messageClient( %client, 'MsgPlayerPopupItem', "", %key, "KickPlayer", "", 'Vote to Kick', 3 );
      }
   }


   // Admin only options on players:
   else if ( %isAdmin && !isDemo() )
   {
      if ( !%isTargetBot && !%isTargetAdmin )
	 messageClient( %client, 'MsgPlayerPopupItem', "", %key, "AdminPlayer", "", 'Make Admin', 2 );

      if ( !%isTargetSelf && %outrankTarget )
      {
	 messageClient( %client, 'MsgPlayerPopupItem', "", %key, "KickPlayer", "", 'Kick', 3 );

	 if ( !%isTargetBot )
	 {
	    if( %client.isSuperAdmin )
	       messageClient( %client, 'MsgPlayerPopupItem', "", %key, "BanPlayer", "", 'Ban', 4 );

	    if ( !%isTargetObserver )
	       messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ToObserver", "", 'Force observer', 5 );
	 }
      }


      if ( %isTargetSelf || %outrankTarget )
      {
	 if ( %game.numTeams > 1 )
	 {
	    if ( %isTargetObserver )
	    {
	       %action = %isTargetSelf ? "Join " : "Change to ";
	       %str1 = %action @ getTaggedString( %game.getTeamName(1) );
	       %str2 = %action @ getTaggedString( %game.getTeamName(2) );

	       messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ChangeTeam", "", %str1, 6 );
	       messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ChangeTeam", "", %str2, 7 );
	    }
	    else
	    {
	       %changeTo = %targetClient.team == 1 ? 2 : 1;
	       %str = "Switch to " @ getTaggedString( %game.getTeamName(%changeTo) );
	       %caseId = 5 + %changeTo;

	       messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ChangeTeam", "", %str, %caseId );
	    }
	 }
	 else if ( %isTargetObserver )
	 {
	    %str = %isTargetSelf ? 'Join the Game' : 'Add to Game';
	    messageClient( %client, 'MsgPlayerPopupItem', "", %key, "JoinGame", "", %str, 8 );
	 }
      }
   }
}

//------------------------------------------------------------------------------
function DefaultGame::sendGameVoteMenu( %game, %client, %key )
{
   %isAdmin = (%client.isAdmin || %client.isSuperAdmin);
   %isSuperAdmin = (%client.isSuperAdmin);
   %multipleTeams = %game.numTeams > 1;

	// jailed players do not need menus, unless they are admins
	// Note that this does not block gametype-specific menus
	if (%client.isJailed && !%isAdmin)
		return;

   // no one is going anywhere until this thing starts
   if($MatchStarted)
   {
      // Client options:
      if ( %client.team != 0 )
      {
	 if ( %multipleTeams )
	    if( !$Host::TournamentMode )
	       messageClient( %client, 'MsgVoteItem', "", %key, 'ChooseTeam', "", 'Change your Team' );
	 messageClient( %client, 'MsgVoteItem', "", %key, 'MakeObserver', "", 'Become an Observer' );
      }
      else
      {
	 if(!%multipleTeams && !$Host::TournamentMode)
	    messageClient( %client, 'MsgVoteItem', "", %key, 'JoinGame', "", 'Join the Game' );
      }

      //%totalSlots = $Host::maxPlayers - ($HostGamePlayerCount + $HostGameBotCount);
     // if( $HostGameBotCount > 0 && %totalSlots > 0 && %isAdmin)
	 //messageClient( %client, 'MsgVoteItem', "", %key, 'Addbot', "", 'Add a Bot' );
   }

   if( !%client.canVote && !%isAdmin )
      return;

   if (isDemo())
      return;

   if ( %game.scheduleVote $= "" )
   {
      if(!%isAdmin)
      {
	 // Actual vote options:
	 messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChangeMission', 'change the mission to', 'Vote to Change the Mission' );

	 if( $Host::TournamentMode )
	 {
	    messageClient( %client, 'MsgVoteItem', "", %key, 'VoteFFAMode', 'Change server to Free For All.', 'Vote Free For All Mode' );

	    if(!$MatchStarted && !$CountdownStarted)
	       messageClient( %client, 'MsgVoteItem', "", %key, 'VoteMatchStart', 'Start Match', 'Vote to Start the Match' );
	 }
	 else
	    messageClient( %client, 'MsgVoteItem', "", %key, 'VoteTournamentMode', 'Change server to Tournament.', 'Vote Tournament Mode' );

	 if ( %multipleTeams )
	 {
	    if(!$MatchStarted && !$Host::TournamentMode)
	       messageClient( %client, 'MsgVoteItem', "", %key, 'ChooseTeam', "", 'Change your Team' );
	 }

	    if ( $teamDamage )
	       messageClient( %client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'disable team damage', 'Vote to Disable Team Damage' );
	    else
	       messageClient( %client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'enable team damage', 'Vote to Enable Team Damage' );

	 if ( $Host::Purebuild == 1 )
		messageClient( %client, 'MsgVoteItem', "", %key, 'VotePurebuild', 'disable pure building', '[\c1pure\c0] Vote to Disable Pure Building' );
	 else
		messageClient( %client, 'MsgVoteItem', "", %key, 'VotePurebuild', 'enable pure building', '[\c1pure\c0] Vote to Enable Pure Building' );
	if ( $Host::ExpertMode == 1)
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteExpertMode', 'disable expert mode', '[\c1pure\c0] Vote to Disable Expert Mode' );
	else
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteExpertMode', 'enable expert mode', '[\c1pure\c0] Vote to Enable Expert Mode' );
      }
      else
      {
	 // Actual vote options:
	 messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChangeMission', 'change the mission to', 'Change the Mission' );

	 if( $Host::TournamentMode )
	 {
	    messageClient( %client, 'MsgVoteItem', "", %key, 'VoteFFAMode', 'Change server to Free For All.', 'Free For All Mode' );

	    if(!$MatchStarted && !$CountdownStarted)
	       messageClient( %client, 'MsgVoteItem', "", %key, 'VoteMatchStart', 'Start Match', 'Start Match' );
	 }
	 else
	    messageClient( %client, 'MsgVoteItem', "", %key, 'VoteTournamentMode', 'Change server to Tournament.', 'Tournament Mode' );

	 if ( %multipleTeams )
	 {
	    if(!$MatchStarted)
	       messageClient( %client, 'MsgVoteItem', "", %key, 'ChooseTeam', "", 'Choose Team' );
	 }

	    if ( $teamDamage )
	       messageClient( %client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'disable team damage', 'Disable Team Damage' );
	    else
	       messageClient( %client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'enable team damage', 'Enable Team Damage' );

	  if ( $Host::Purebuild == 1 )
	     messageClient( %client, 'MsgVoteItem', "", %key, 'VotePurebuild', 'disable pure building', '[\c1pure\c0] Disable Pure Building' );
	  else
	     messageClient( %client, 'MsgVoteItem', "", %key, 'VotePurebuild', 'enable pure building', '[\c1pure\c0] Enable Pure Building' );
      }
   }

   // Admin only options (plus some votable options :P) :
	if ( %isAdmin ) {
		if ( $Host::Cascade == 1)
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteCascadeMode', 'disable cascade mode', '[\c1pure\c0] Disable Cascade Mode' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteCascadeMode', 'enable cascade mode', '[\c1pure\c0] Enable Cascade Mode' );
		if ( $Host::ExpertMode == 1)
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteExpertMode', 'disable expert mode', '[\c1pure\c0] Disable Expert Mode' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteExpertMode', 'enable expert mode', '[\c1pure\c0] Enable Expert Mode' );
		if ( $Host::Vehicles == 1)
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteVehicles', 'disable vehicles', '[\c1pure\c0] Disable Vehicles' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteVehicles', 'enable vehicles', '[\c1pure\c0] Enable Vehicles' );
		if ( $Host::InvincibleArmors == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteInvincibleArmors', 'disable invincible armors', '[\c1pure\c0] Disable Invincible Armors' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteInvincibleArmors', 'enable invincible armors', '[\c1pure\c0] Enable Invincible Armors' );
		if ( $Host::InvincibleDeployables == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteInvincibleDeployables', 'disable invincible deployables', '[\c1pure\c0] Disable Invincible Deployables' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteInvincibleDeployables', 'enable invincible deployables', '[\c1pure\c0] Enable Invincible Deployables' );
		if ( $Host::AllowUnderground == 1)
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteUndergroundMode', 'disable underground mode', '[\c1pure\c0] Disable Underground Mode' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteUndergroundMode', 'enable underground mode', '[\c1pure\c0] Enable Underground Mode' );
		if ( $Host::Hazard::Enabled == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteHazardMode', 'disable hazard mode', '[\c1hazard\c0] Disable Hazard Mode' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteHazardMode', 'enable hazard mode', '[\c1hazard\c0] Enable Hazard Mode' );
		if ($MTC_Loaded == 1) {
			if ( $Host::MTC::Enabled == 1 )
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteMTCMode', 'disable MTC mode', '[\c1mtc\c0] Disable MTC Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteMTCMode', 'enable MTC mode', '[\c1mtc\c0] Enable MTC Mode' );
		}
		if ( $Host::SatchelChargeEnabled == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSatchelCharge', 'disable satchel charges', '[\c1security\c0] Disable Satchel Charges' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSatchelCharge', 'enable satchel charges', '[\c1security\c0] Enable Satchel Charges' );
		if ( $Host::OnlyOwnerDeconstruct == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteOnlyOwnerDeconstruct', 'disable only owner deconstruct', '[\c1security\c0] Disable Only Owner Deconstruct' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteOnlyOwnerDeconstruct', 'enable only owner deconstruct', '[\c1security\c0] Enable Only Owner Deconstruct' );
		if ( $Host::OnlyOwnerCascade == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteOnlyOwnerCascade', 'disable only owner cascade', '[\c1security\c0] Disable Only Owner Cascade' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteOnlyOwnerCascade', 'enable only owner cascade', '[\c1security\c0] Enable Only Owner Cascade' );
		if ( $Host::OnlyOwnerRotate == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteOnlyOwnerRotate', 'disable only owner rotate', '[\c1security\c0] Disable Only Owner Rotate' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteOnlyOwnerRotate', 'enable only owner rotate', '[\c1security\c0] Enable Only Owner Rotate' );
		if ( $Host::OnlyOwnerCubicReplace == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteOnlyOwnerCubicReplace', 'disable only owner cubic-replace', '[\c1security\c0] Disable Only Owner Cubic-Replace' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteOnlyOwnerCubicReplace', 'enable only owner cubic-replace', '[\c1security\c0] Enable Only Owner Cubic-Replace' );
		if ( $Host::Prison::Enabled == 1 )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VotePrison', 'disable prison', '[\c1prison\c0] Disable Prison' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VotePrison', 'enable prison', '[\c1prison\c0] Enable Prison' );
		if ( $Host::Prison::Enabled == 1 ) {
			if ( $Host::Prison::Kill == 1 )
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePrisonKilling', 'disable jailing killers', '[\c1prison\c0] Disable Jailing of Killers' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePrisonKilling', 'enable jailing killers', '[\c1prison\c0] Enable Jailing of Killers' );
			if ( $Host::Prison::TeamKill == 1 )
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePrisonTeamKilling', 'disable jailing team killers', '[\c1prison\c0] Disable Jailing of Team Killers' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePrisonTeamKilling', 'enable jailing team killers', '[\c1prison\c0] Enable Jailing of Team Killers' );
			if ( $Host::Prison::DeploySpam == 1 )
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePrisonDeploySpam', 'disable jailing deploy spammers', '[\c1prison\c0] Disable Jailing of Deploy Spammers');
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePrisonDeploySpam', 'enable jailing deploy spammers', '[\c1prison\c0] Enable Jailing of Deploy Spammers' );
		}
		if ($Host::Nerf::Enabled == 1)
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteNerfWeapons', 'disable nerf weapons', '[\c1nerf\c0] Disable Nerf Weapons' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteNerfWeapons', 'enable nerf weapons', '[\c1nerf\c0] Enable Nerf Weapons' );
		if ($Host::Nerf::Enabled == 1) {
			if ( $Host::Nerf::DanceAnim == 1 && $Host::Nerf::DeathAnim == 0 )
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteNerfDance', 'disable nerf dance mode', '[\c1nerf\c0] Disable Nerf Dance Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteNerfDance', 'enable nerf dance mode', '[\c1nerf\c0] Enable Nerf Dance Mode' );
			if ( $Host::Nerf::DeathAnim == 1 )
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteNerfDeath', 'disable nerf death mode', '[\c1nerf\c0] Disable Nerf Death Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteNerfDeath', 'enable nerf death mode', '[\c1nerf\c0] Enable Nerf Death Mode' );
			if ($Host::Prison::Enabled == 1) {
				if ( $Host::Nerf::Prison == 1 )
					messageClient( %client, 'MsgVoteItem', "", %key, 'VoteNerfPrison', 'disable nerf prison mode', '[\c1nerf\c0] Disable Nerf Prison Mode' );
				else
					messageClient( %client, 'MsgVoteItem', "", %key, 'VoteNerfPrison', 'enable nerf prison mode', '[\c1nerf\c0] Enable Nerf Prison Mode' );
			}
		}
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteGlobalPowerCheck', 'evaluate power for all deployables', '[\c1power\c0] Evaluate Power for All Deployables' );
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteRemoveDupDeployables', 'remove all duplicate deployables', '[\c4spam\c0] Remove All Duplicate Deployables' );
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteRemoveNonPoweredDeployables', 'remove all deployables without power', '[\c4spam\c0] Remove All Deployables Without Power' );
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteRemoveOrphanedDeployables', 'remove all orphaned deployables', '[\c4spam\c0] Remove All Orphaned Deployables' );
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteRemoveDeployables', 'remove all deployables in mission', '[\c4spam\c0] Remove All Deployables In Mission' );
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChangeTimeLimit', 'change the time limit', 'Change the Time Limit' );
		messageClient( %client, 'MsgVoteItem', "", %key, 'VoteResetServer', 'reset server defaults', 'Reset the Server' );
	}
}

//------------------------------------------------------------------------------
function DefaultGame::sendGameTeamList( %game, %client, %key )
{
   %teamCount = %game.numTeams;
   if ( %teamCount < 2 )
   {
      warn( "Team menu requested for one-team game!" );
      return;
   }

   for ( %team = 1; %team - 1 < %teamCount; %team++ )
      messageClient( %client, 'MsgVoteItem', "", %key, %team, "", detag( getTaggedString( %game.getTeamName(%team) ) ) );
}

//------------------------------------------------------------------------------
function DefaultGame::sendTimeLimitList( %game, %client, %key )
{
   messageClient( %client, 'MsgVoteItem', "", %key, 10, "", '10 minutes' );
   messageClient( %client, 'MsgVoteItem', "", %key, 15, "", '15 minutes' );
   messageClient( %client, 'MsgVoteItem', "", %key, 20, "", '20 minutes' );
   messageClient( %client, 'MsgVoteItem', "", %key, 25, "", '25 minutes' );
   messageClient( %client, 'MsgVoteItem', "", %key, 30, "", '30 minutes' );
   messageClient( %client, 'MsgVoteItem', "", %key, 45, "", '45 minutes' );
   messageClient( %client, 'MsgVoteItem', "", %key, 60, "", '60 minutes' );
   messageClient( %client, 'MsgVoteItem', "", %key, 10080, "", 'No time limit' );
}

//------------------------------------------------------------------------------
// all global votes here
// this function was created to remove the call to "eval", which is non-functional in PURE servers...
function DefaultGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4)
{
   switch$ (%typeName)
   {
      case "voteChangeMission":
	 %game.voteChangeMission(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteTeamDamage":
	 %game.voteTeamDamage(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteTournamentMode":
	 %game.voteTournamentMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteMatchStart":
	 %game.voteMatchStart(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteFFAMode":
	 %game.voteFFAMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteChangeTimeLimit":
	 %game.voteChangeTimeLimit(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteResetServer":
	 %game.voteResetServer(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteKickPlayer":
	 %game.voteKickPlayer(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteAdminPlayer":
	 %game.voteAdminPlayer(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteGreedMode":
	 %game.voteGreedMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteHoardMode":
	 %game.voteHoardMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "votePurebuild":
	 %game.votePurebuild(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteCascadeMode":
	 %game.voteCascadeMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteExpertMode":
	 %game.voteExpertMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "VoteVehicles":
	 %game.VoteVehicles(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteInvincibleArmors":
	 %game.voteInvincibleArmors(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteInvincibleDeployables":
	 %game.voteInvincibleDeployables(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteUndergroundMode":
	 %game.voteUndergroundMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteHazardMode":
	 %game.voteHazardMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteMTCMode":
	 %game.voteMTCMode(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteSatchelCharge":
	 %game.voteSatchelCharge(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteOnlyOwnerDeconstruct":
	 %game.voteOnlyOwnerDeconstruct(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteOnlyOwnerCascade":
	 %game.voteOnlyOwnerCascade(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteOnlyOwnerRotate":
	 %game.voteOnlyOwnerRotate(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteOnlyOwnerCubicReplace":
	 %game.voteOnlyOwnerCubicReplace(%admin, %arg1, %arg2, %arg3, %arg4);
      case "votePrison":
	 %game.votePrison(%admin, %arg1, %arg2, %arg3, %arg4);
      case "votePrisonKilling":
	 %game.VotePrisonKilling(%admin, %arg1, %arg2, %arg3, %arg4);
      case "votePrisonTeamKilling":
	 %game.VotePrisonTeamKilling(%admin, %arg1, %arg2, %arg3, %arg4);
      case "votePrisonDeploySpam":
	 %game.VotePrisonDeploySpam(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteNerfWeapons":
	 %game.voteNerfWeapons(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteNerfDance":
	 %game.voteNerfDance(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteNerfDeath":
	 %game.voteNerfDeath(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteNerfPrison":
	 %game.voteNerfPrison(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteGlobalPowerCheck":
	 %game.voteGlobalPowerCheck(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteRemoveDupDeployables":
	 %game.voteRemoveDupDeployables(%admin, %arg1, %arg2, %arg3, %arg4);
      case "VoteRemoveNonPoweredDeployables":
	 %game.VoteRemoveNonPoweredDeployables(%admin, %arg1, %arg2, %arg3, %arg4);
      case "VoteRemoveOrphanedDeployables":
	 %game.VoteRemoveOrphanedDeployables(%admin, %arg1, %arg2, %arg3, %arg4);
      case "voteRemoveDeployables":
	 %game.voteRemoveDeployables(%admin, %arg1, %arg2, %arg3, %arg4);
   }
}

function DefaultGame::voteChangeMission(%game, %admin, %missionDisplayName, %typeDisplayName, %missionId, %missionTypeId)
{
	if (%admin) {
		if (%missionTypeId $= "LoadBuildingFile") {
			%file = stripChars(%missionDisplayName,":\\/");
			if (getSubStr(%file,0,1) $= "_") {
				%dir = $SaveBuilding::AutoSaveFolder;
				%file = getSubStr(%file,1,strLen(%file)-1);
			}
			else
				%dir = "Buildings/Admin/";
			if (%file $= "")
				return;
			else {
				if (strStr(%file,"..") != -1)
					return;
			}
			%file = %dir @ %file;
			if (isFile(%file) && getSubStr(%file,strLen(%file)-3,3) $= ".cs") {
				// Message is sent first, so clients know what happened in case server crashes
				messageAll('MsgAdminForce', '\c2The Admin has loaded a building file.');
				compile(%file);
				exec(%file);
			}
			return;
		}
	}

   %mission = $HostMissionFile[%missionId];
   if ( %mission $= "" )
   {
      error( "Invalid mission index passed to DefaultGame::voteChangeMission!" );
      return;
   }

   %missionType = $HostTypeName[%missionTypeId];
   if ( %missionType $= "" )
   {
      error( "Invalid mission type id passed to DefaultGame::voteChangeMission!" );
      return;
   }

   if(%admin)
   {
      messageAll('MsgAdminChangeMission', '\c2The Admin has changed the mission to %1 (%2).', %missionDisplayName, %typeDisplayName );
      logEcho("mission changed to "@%missionDisplayName@"/"@%typeDisplayName@" (admin)");
      %game.gameOver();
      loadMission( %mission, %missionType, false );
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 messageAll('MsgVotePassed', '\c2The mission was changed to %1 (%2) by vote.', %missionDisplayName, %typeDisplayName );
	 logEcho("mission changed to "@%missionDisplayName@"/"@%typeDisplayName@" (vote)");
	 %game.gameOver();
	 loadMission( %mission, %missionType, false );
      }
      else
	 messageAll('MsgVoteFailed', '\c2Change mission vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
   }
}

//------------------------------------------------------------------------------
function DefaultGame::voteTeamDamage(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($teamDamage)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled team damage.');
	 $Host::TeamDamageOn = $TeamDamage = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled team damage.');
	 $Host::TeamDamageOn = $TeamDamage = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($teamDamage)
	 {
	    messageAll('MsgVotePassed', '\c2Team damage was disabled by vote.');
	    $Host::TeamDamageOn = $TeamDamage = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Team damage was enabled by vote.');
	    $Host::TeamDamageOn = $TeamDamage = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($teamDamage)
	    messageAll('MsgVoteFailed', '\c2Disable team damage vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable team damage vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("team damage "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::votePurebuild(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Purebuild == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled pure building.');
	 purebuildOff();
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled pure building.');
	 purebuildOn();
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Purebuild == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Pure building was disabled by vote.');
	    purebuildOff();
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Pure building was enabled by vote.');
	    purebuildOn();
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Purebuild == 1)
	    messageAll('MsgVoteFailed', '\c2Disable pure building vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable pure building vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("purebuild "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteCascadeMode(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Cascade == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled Cascade mode.');
         $Host::Cascade = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled Cascade mode.');
         $Host::Cascade = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Cascade == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Cascade mode was disabled by vote.');
            $Host::Cascade = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Cascade mode was enabled by vote.');
            $Host::Cascade = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Cascade == 1 && $Host::Cascade == 0)
	    messageAll('MsgVoteFailed', '\c2Disable Cascade mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable Cascade mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("cascade mode "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteExpertMode(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::ExpertMode == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled Expert mode.');
         expertModeOff();
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled Expert mode.');
         expertModeOn();
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::ExpertMode == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Expert mode was disabled by vote.');
            expertModeOff();
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Expert mode was enabled by vote.');
            expertModeOn();
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::ExpertMode == 1 && $Host::ExpertMode == 0)
	    messageAll('MsgVoteFailed', '\c2Disable Expert mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable Expert mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("expert mode "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteVehicles(%game, %admin) {
	if (!(Game.pureVehTime < getSimTime()))
		return;

	%setto = "";
	%cause = "";
	if(%admin) {
		if($Host::Vehicles == 1) {
			messageAll('MsgAdminForce', '\c2The Admin has disabled vehicles.');
			disableVehicles();
			Game.pureVehTime = getSimTime() + 5000;
			%setto = "disabled";
		}
		else {
			messageAll('MsgAdminForce', '\c2The Admin has enabled vehicles.');
			enableVehicles();
			Game.pureVehTime = getSimTime() + 5000;
			%setto = "enabled";
		}
		%cause = "(admin)";
	}
	else {
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100)) {
			if($Host::Vehicles == 1) {
				messageAll('MsgVotePassed', '\c2Vehicles were disabled by vote.');
				disableVehicles();
				Game.pureVehTime = getSimTime() + 5000;
				%setto = "disabled";
			}
			else {
				messageAll('MsgVotePassed', '\c2Vehicles were enabled by vote.');
				enableVehicles();
				Game.pureVehTime = getSimTime() + 5000;
				%setto = "enabled";
			}
			%cause = "(vote)";
		}
		else {
			if($Host::Vehicles == 1)
				messageAll('MsgVoteFailed', '\c2Disable vehicles vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
			else
				messageAll('MsgVoteFailed', '\c2Enable vehicles vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
		}
	}
	if(%setto !$= "")
		logEcho("pure vehicles "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteSatchelCharge(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::SatchelChargeEnabled == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled satchel charges.');
	 $Host::SatchelChargeEnabled = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled satchel charges.');
	 $Host::SatchelChargeEnabled = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::SatchelChargeEnabled == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Satchel charges was disabled by vote.');
	    $Host::SatchelChargeEnabled = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Satchel charges was enabled by vote.');
	    $Host::SatchelChargeEnabled = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::SatchelChargeEnabled == 1)
	    messageAll('MsgVoteFailed', '\c2Disable satchel charges vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable satchel charges vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("satchel charges "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteOnlyOwnerDeconstruct(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::OnlyOwnerDeconstruct == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled only owner deconstruct mode.');
	 $Host::OnlyOwnerDeconstruct = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled only owner deconstruct mode.');
	 $Host::OnlyOwnerDeconstruct = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::OnlyOwnerDeconstruct == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Only owner deconstruct mode was disabled by vote.');
	    $Host::OnlyOwnerDeconstruct = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Only owner deconstruct mode was enabled by vote.');
	    $Host::OnlyOwnerDeconstruct = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::OnlyOwnerDeconstruct == 1)
	    messageAll('MsgVoteFailed', '\c2Only owner deconstruct vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Only owner deconstruct vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("only owner deconstruct "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteOnlyOwnerCascade(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::OnlyOwnerCascade == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled only owner cascade mode.');
	 $Host::OnlyOwnerCascade = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled only owner cascade mode.');
	 $Host::OnlyOwnerCascade = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::OnlyOwnerCascade == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Only owner cascade mode was disabled by vote.');
	    $Host::OnlyOwnerCascade = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Only owner cascade mode was enabled by vote.');
	    $Host::OnlyOwnerCascade = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::OnlyOwnerCascade == 1)
	    messageAll('MsgVoteFailed', '\c2Only owner cascade vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Only owner cascade vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("only owner cascade "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteOnlyOwnerRotate(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::OnlyOwnerRotate == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled only owner rotate mode.');
	 $Host::OnlyOwnerRotate = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled only owner rotate mode.');
	 $Host::OnlyOwnerRotate = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::OnlyOwnerRotate == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Only owner rotate mode was disabled by vote.');
	    $Host::OnlyOwnerRotate = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Only owner rotate mode was enabled by vote.');
	    $Host::OnlyOwnerRotate = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::OnlyOwnerRotate == 1)
	    messageAll('MsgVoteFailed', '\c2Only owner rotate vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Only owner rotate vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("only owner rotate "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteOnlyOwnerCubicReplace(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::OnlyOwnerCubicReplace == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled only owner cubic-replace mode.');
	 $Host::OnlyOwnerCubicReplace = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled only owner cubic-replace mode.');
	 $Host::OnlyOwnerCubicReplace = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::OnlyOwnerCubicReplace == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Only owner cubic-replace mode was disabled by vote.');
	    $Host::OnlyOwnerCubicReplace = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Only owner cubic-replace mode was enabled by vote.');
	    $Host::OnlyOwnerCubicReplace = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::OnlyOwnerCubicReplace == 1)
	    messageAll('MsgVoteFailed', '\c2Only owner cubic-replace vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Only owner cubic-replace vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("only owner cubic-replace "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteGlobalPowerCheck( %game, %admin, %client ) {
	%cause = "";
	if (%admin) {
		messageAll( 'MsgAdminForce', '\c2The Admin has evaluated power for all deployables in the mission.' );
		globalPowerCheck();
		%cause = "(admin)";
	}
	else {
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100)) {
			messageAll('MsgVotePassed', '\c2Evaluating power for all deployables in the mission by vote.' );
			globalPowerCheck();
			%cause = "(vote)";
		}
		else
			messageAll('MsgVoteFailed', '\c2The vote to evaluate power for all deployables in the mission did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	}
	if(%cause !$= "")
		logEcho("evaluate power for all deployables "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteRemoveDupDeployables( %game, %admin, %client ) {
	if (!(Game.removeDepTime < getSimTime()))
		return;
	%cause = "";
	if (%admin) {
		messageAll( 'MsgAdminForce', '\c2The Admin removed all duplicate deployables in the mission.' );
		Game.removeDepTime = getSimTime() + delDupPieces(0,0,true) + 1000;
		%cause = "(admin)";
	}
	else {
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100)) {
			messageAll('MsgVotePassed', '\c2Removing all duplicate deployables in the mission by vote.' );
			delDupPieces(0,0,true);
			Game.removeDepTime = getSimTime() + delDupPieces(0,0,true) + 1000;
			%cause = "(vote)";
		}
		else
			messageAll('MsgVoteFailed', '\c2The vote to remove all duplicate deployables in the mission did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	}
	if(%cause !$= "")
		logEcho("remove all duplicate deployables "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteRemoveNonPoweredDeployables( %game, %admin, %client ) {
	if (!(Game.removeDepTime < getSimTime()))
		return;
	%cause = "";
	if (%admin) {
		messageAll( 'MsgAdminForce', '\c2The Admin removed all deployables in the mission without power.' );
		Game.removeDepTime = getSimTime() + delNonPoweredPieces(true) + 1000;
		%cause = "(admin)";
	}
	else {
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100)) {
			messageAll('MsgVotePassed', '\c2Removing all deployables in the mission without power by vote.' );
			delNonPoweredPieces(true);
			Game.removeDepTime = getSimTime() + delNonPoweredPieces(true) + 1000;
			%cause = "(vote)";
		}
		else
			messageAll('MsgVoteFailed', '\c2The vote to remove all deployables in the mission without power did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	}
	if(%cause !$= "")
		logEcho("remove all deployables without power "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteRemoveOrphanedDeployables( %game, %admin, %client ) {
	if (!(Game.removeDepTime < getSimTime()))
		return;
	%cause = "";
	if (%admin) {
		messageAll( 'MsgAdminForce', '\c2The Admin removed all orphaned deployables in the mission.' );
		Game.removeDepTime = getSimTime() + delOrphanedPieces(true) + 1000;
		%cause = "(admin)";
	}
	else {
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100)) {
			messageAll('MsgVotePassed', '\c2Removing all orphaned deployables in the mission by vote.' );
			delOrphanedPieces(true);
			Game.removeDepTime = getSimTime() + delOrphanedPieces(true) + 1000;
			%cause = "(vote)";
		}
		else
			messageAll('MsgVoteFailed', '\c2The vote to remove all orphaned deployables in the mission did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	}
	if(%cause !$= "")
		logEcho("remove all orphaned deployables "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteRemoveDeployables( %game, %admin, %client ) {
	if (!(Game.removeDepTime < getSimTime()))
		return;
	%cause = "";
	if (%admin) {
		messageAll( 'MsgAdminForce', '\c2The Admin removed all deployables in the mission.' );
		Game.removeDepTime = getSimTime() + unpureDeployables() + 1000;
		%cause = "(admin)";
	}
	else {
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100)) {
			messageAll('MsgVotePassed', '\c2Removing all deployables in the mission by vote.' );
			unpureDeployables();
			Game.removeDepTime = getSimTime() + unpureDeployables() + 1000;
			%cause = "(vote)";
		}
		else
			messageAll('MsgVoteFailed', '\c2The vote to remove all deployables in the mission did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	}
	if(%cause !$= "")
		logEcho("remove all deployables "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteInvincibleArmors(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::InvincibleArmors == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled invincible armors.');
	 $Host::InvincibleArmors = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled invincible armors.');
	 $Host::InvincibleArmors = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::InvincibleArmors == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Invincible armors was disabled by vote.');
	    $Host::InvincibleArmors = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Invincible armors was enabled by vote.');
	    $Host::InvincibleArmors = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::InvincibleArmors == 1)
	    messageAll('MsgVoteFailed', '\c2Disable invincible armors vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable invincible armors vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("invincible armors "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VoteInvincibleDeployables(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::InvincibleDeployables == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled invincible deployables.');
	 $Host::InvincibleDeployables = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled invincible deployables.');
	 $Host::InvincibleDeployables = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::InvincibleDeployables == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Invincible deployables was disabled by vote.');
	    $Host::InvincibleDeployables = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Invincible deployables was enabled by vote.');
	    $Host::InvincibleDeployables = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::InvincibleDeployables == 1)
	    messageAll('MsgVoteFailed', '\c2Disable invincible deployables vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable invincible deployables vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("invincible deployables "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteUndergroundMode(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::AllowUnderground == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled Underground mode.');
	 $Host::AllowUnderground = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled Underground mode.');
	 $Host::AllowUnderground = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::AllowUnderground == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Underground mode was disabled by vote.');
	    $Host::AllowUnderground = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Underground mode was enabled by vote.');
	    $Host::AllowUnderground = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::AllowUnderground == 1)
	    messageAll('MsgVoteFailed', '\c2Disable Underground mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable Underground mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("underground mode "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteHazardMode(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Hazard::Enabled == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled Hazard mode.');
	 hazardOff();
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled Hazard mode.');
	 hazardOn();
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Hazard::Enabled == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Hazard mode was disabled by vote.');
	    hazardOff();
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Hazard mode was enabled by vote.');
	    hazardOn();
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Hazard::Enabled == 1)
	    messageAll('MsgVoteFailed', '\c2Disable Hazard mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable Hazard mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("hazard mode "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteMTCMode(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::MTC::Enabled == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled MTC mode.');
	 stopMTC();
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled MTC mode.');
	 startMTC();
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::MTC::Enabled == 1)
	 {
	    messageAll('MsgVotePassed', '\c2MTC mode was disabled by vote.');
	    stopMTC();
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2MTC mode was enabled by vote.');
	    startMTC();
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::MTC::Enabled == 1)
	    messageAll('MsgVoteFailed', '\c2Disable MTC mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable MTC mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("MTC mode "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::votePrison(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Prison::Enabled == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled the prison.');
	 prisonDisable();
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled the prison.');
	 prisonEnable();
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Prison::Enabled == 1)
	 {
	    messageAll('MsgVotePassed', '\c2The prison was disabled by vote.');
	    prisonDisable();
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2The prison was enabled by vote.');
	    prisonEnable();
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Prison::Enabled == 1)
	    messageAll('MsgVoteFailed', '\c2Disable prison vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable prison vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("prison "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VotePrisonKilling(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Prison::Kill == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled jailing of killers.');
	 $Host::Prison::Kill = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled jailing of killers.');
	 $Host::Prison::Kill = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Prison::Kill == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Jailing of killers was disabled by vote.');
	    $Host::Prison::Kill = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Jailing of killers was enabled by vote.');
	    $Host::Prison::Kill = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Prison::Kill == 1)
	    messageAll('MsgVoteFailed', '\c2Disable jailing of killers vote not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable jailing of killers vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("prison killing "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VotePrisonTeamKilling(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Prison::TeamKill == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled jailing of team killers.');
	 $Host::Prison::TeamKill = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled jailing of team killers.');
	 $Host::Prison::TeamKill = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Prison::TeamKill == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Jailing of team killers was disabled by vote.');
	    $Host::Prison::TeamKill = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Jailing of team killers was enabled by vote.');
	    $Host::Prison::TeamKill = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Prison::TeamKill == 1)
	    messageAll('MsgVoteFailed', '\c2Disable jailing of team killers vote not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable jailing of team killers vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("prison team killers "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::VotePrisonDeploySpam(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Prison::DeploySpam == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled jailing of deploy spammers.');
	 $Host::Prison::DeploySpam = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled jailing of deploy spammers.');
	 $Host::Prison::DeploySpam = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Prison::DeploySpam == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Jailing of deploy spammers was disabled by vote.');
	    $Host::Prison::DeploySpam = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Jailing of deploy spammers was enabled by vote.');
	    $Host::Prison::DeploySpam = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Prison::DeploySpam == 1)
	    messageAll('MsgVoteFailed', '\c2Disable jailing of deploy spammers vote not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable jailing of deploy spammers vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("prison deploy spammers "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteNerfWeapons(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Nerf::Enabled == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled nerf weapons.');
	 nerfDisable();
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled nerf weapons.');
	 nerfEnable();
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Nerf::Enabled == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Nerf weapons were disabled by vote.');
	    nerfDisable();
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Nerf weapons were enabled by vote.');
	    nerfEnable();
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Nerf::Enabled == 1)
	    messageAll('MsgVoteFailed', '\c2Disable nerf weapons vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable nerf weapons vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("nerf weapons "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteNerfDance(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Nerf::DanceAnim == 1 && $Host::Nerf::DeathAnim == 0)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled Nerf Dance mode.');
	 $Host::Nerf::DanceAnim = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled Nerf Dance mode.');
	 $Host::Nerf::DanceAnim = 1;
	 $Host::Nerf::DeathAnim = 0;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Nerf::DanceAnim == 1 && $Host::Nerf::DeathAnim == 0)
	 {
	    messageAll('MsgVotePassed', '\c2Nerf Dance mode was disabled by vote.');
	    $Host::Nerf::DanceAnim = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Nerf Dance mode was enabled by vote.');
	    $Host::Nerf::DanceAnim = 1;
	    $Host::Nerf::DeathAnim = 0;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Nerf::DanceAnim == 1 && $Host::Nerf::DeathAnim == 0)
	    messageAll('MsgVoteFailed', '\c2Disable Nerf Dance mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable Nerf Dance mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("nerf dance mode "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteNerfDeath(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Nerf::DeathAnim == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled Nerf Death mode.');
	 $Host::Nerf::DeathAnim = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled Nerf Death mode.');
	 $Host::Nerf::DanceAnim = 0;
	 $Host::Nerf::DeathAnim = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Nerf::DeathAnim == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Nerf Death mode was disabled by vote.');
	    $Host::Nerf::DeathAnim = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Nerf Death mode was enabled by vote.');
	    $Host::Nerf::DeathAnim = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Nerf::DeathAnim == 1 && $Host::Nerf::DeathAnim == 0)
	    messageAll('MsgVoteFailed', '\c2Disable Nerf Death mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable Nerf Death mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("nerf death mode "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteNerfPrison(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(%admin)
   {
      if($Host::Nerf::Prison == 1)
      {
	 messageAll('MsgAdminForce', '\c2The Admin has disabled Nerf Prison mode.');
	 $Host::Nerf::Prison = 0;
	 %setto = "disabled";
      }
      else
      {
	 messageAll('MsgAdminForce', '\c2The Admin has enabled Nerf Prison mode.');
	 $Host::Nerf::Prison = 1;
	 %setto = "enabled";
      }
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 if($Host::Nerf::Prison == 1)
	 {
	    messageAll('MsgVotePassed', '\c2Nerf Prison mode was disabled by vote.');
	    $Host::Nerf::Prison = 0;
	    %setto = "disabled";
	 }
	 else
	 {
	    messageAll('MsgVotePassed', '\c2Nerf Prison mode was enabled by vote.');
	    $Host::Nerf::Prison = 1;
	    %setto = "enabled";
	 }
	 %cause = "(vote)";
      }
      else
      {
	 if($Host::Nerf::Prison == 1)
	    messageAll('MsgVoteFailed', '\c2Disable Nerf Prison mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 else
	    messageAll('MsgVoteFailed', '\c2Enable Nerf Prison mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }
   if(%setto !$= "")
      logEcho("nerf prison mode "@%setto SPC %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteTournamentMode( %game, %admin, %missionDisplayName, %typeDisplayName, %missionId, %missionTypeId )
{
   %mission = $HostMissionFile[%missionId];
   if ( %mission $= "" )
   {
      error( "Invalid mission index passed to DefaultGame::voteTournamentMode!" );
      return;
   }

   %missionType = $HostTypeName[%missionTypeId];
   if ( %missionType $= "" )
   {
      error( "Invalid mission type id passed to DefaultGame::voteTournamentMode!" );
      return;
   }

   %cause = "";
   if (%admin)
   {
      messageAll( 'MsgAdminForce', '\c2The Admin has switched the server to Tournament mode (%1).', %missionDisplayName );
      setModeTournament( %mission, %missionType );
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 messageAll('MsgVotePassed', '\c2Server switched to Tournament mode by vote (%1): %2 percent.', %missionDisplayName, mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	 setModeTournament( %mission, %missionType );
	 %cause = "(vote)";
      }
      else
	 messageAll('MsgVoteFailed', '\c2Tournament mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
   }
   if(%cause !$= "")
      logEcho("tournament mode set "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteMatchStart( %game, %admin)
{
   %cause = "";
   %ready = forceTourneyMatchStart();
   if(%admin)
   {
      if(!%ready)
      {
	 messageClient( %client, 'msgClient', '\c2No players are ready yet.');
	 return;
      }
      else
      {
	 messageAll('msgMissionStart', '\c2The admin has forced the match to start.');
	 %cause = "(admin)";
	 startTourneyCountdown();
      }
   }
   else
   {
      if(!%ready)
      {
	 messageAll( 'msgClient', '\c2Vote passed to start match, but no players are ready yet.');
	 return;
      }
      else
      {
	 %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
	 if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
	 {
	    messageAll('MsgVotePassed', '\c2The match has been started by vote: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
	    startTourneyCountdown();
	 }
	 else
	    messageAll('MsgVoteFailed', '\c2Start Match vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
      }
   }

   if(%cause !$= "")
      logEcho("start match "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteFFAMode( %game, %admin, %client )
{
   %cause = "";
   %name = getTaggedString(%client.name);

   if (%admin)
   {
      messageAll('MsgAdminForce', '\c2The Admin has switched the server to Free For All mode.', %client);
      setModeFFA($CurrentMission, $CurrentMissionType);
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 messageAll('MsgVotePassed', '\c2Server switched to Free For All mode by vote.', %client);
	 setModeFFA($CurrentMission, $CurrentMissionType);
	 %cause = "(vote)";
      }
      else
	 messageAll('MsgVoteFailed', '\c2Free For All mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
   }
   if(%cause !$= "")
      logEcho("free for all set "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteChangeTimeLimit( %game, %admin, %newLimit )
{
   if( %newLimit == 999 )
      %display = "unlimited";
   else
      %display = %newLimit;

   %cause = "";
   if ( %admin )
   {
      messageAll( 'MsgAdminForce', '\c2The Admin changed the mission time limit to %1 minutes.', %display );
      $Host::TimeLimit = %newLimit;
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 messageAll('MsgVotePassed', '\c2The mission time limit was set to %1 minutes by vote.', %display);
	 $Host::TimeLimit = %newLimit;
	 %cause = "(vote)";
      }
      else
	 messageAll('MsgVoteFailed', '\c2The vote to change the mission time limit did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
   }

   //if the time limit was actually changed...
   if(%cause !$= "")
   {
      logEcho("time limit set to "@%display SPC %cause);

      //if the match has been started, reset the end of match countdown
      if ($matchStarted)
      {
	 //schedule the end of match countdown
	 %elapsedTimeMS = getSimTime() - $missionStartTime;
	 %curTimeLeftMS = ($Host::TimeLimit * 60 * 1000) - %elapsedTimeMS;
			error("time limit="@$Host::TimeLimit@", elapsed="@(%elapsedTimeMS / 60000)@", curtimeleftms="@%curTimeLeftMS);
	 CancelEndCountdown();
	 EndCountdown(%curTimeLeftMS);
	 cancel(%game.timeSync);
	 %game.checkTimeLimit(true);
      }
   }
}

//------------------------------------------------------------------------------
function DefaultGame::voteResetServer( %game, %admin, %client )
{
   %cause = "";
   if ( %admin )
   {
      messageAll( 'AdminResetServer', '\c2The Admin has reset the server.' );
      resetServerDefaults();
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 messageAll('MsgVotePassed', '\c2The Server has been reset by vote.' );
	 resetServerDefaults();
	 %cause = "(vote)";
      }
      else
	 messageAll('MsgVoteFailed', '\c2The vote to reset Server to defaults did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
   }
   if(%cause !$= "")
      logEcho("server reset "@%cause);
}

//------------------------------------------------------------------------------
// all team based votes here
function DefaultGame::voteKickPlayer(%game, %admin, %client)
{
   %cause = "";

   if(%admin)
   {
      kick(%client, %admin, %client.guid );
      %cause = "(admin)";
   }
   else
   {
      %team = %client.team;
      %totalVotes = %game.votesFor[%game.kickTeam] + %game.votesAgainst[%game.kickTeam];
      if(%totalVotes > 0 && (%game.votesFor[%game.kickTeam] / %totalVotes) > ($Host::VotePasspercent / 100))
      {
	 kick(%client, %admin, %game.kickGuid);
	 %cause = "(vote)";
      }
      else
      {
	 for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ )
	 {
	    %cl = ClientGroup.getObject( %idx );

	    if (%cl.team == %game.kickTeam && !%cl.isAIControlled())
	       messageClient( %cl, 'MsgVoteFailed', '\c2Kick player vote did not pass' );
	 }
      }
   }

   %game.kickTeam = "";
   %game.kickGuid = "";
   %game.kickClientName = "";

   if(%cause !$= "")
      logEcho(%name@" (cl " @ %game.kickClient @ ") kicked " @ %cause);
}

//------------------------------------------------------------------------------
function DefaultGame::banPlayer(%game, %admin, %client)
{
   %cause = "";
   %name = %client.nameBase;
   if( %admin )
   {
      ban( %client, %admin );
      %cause = "(admin)";
   }

   if(%cause !$= "")
      logEcho(%name@" (cl "@%client@") banned "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::voteAdminPlayer(%game, %admin, %client)
{
   %cause = "";

   if (%admin)
   {
      messageAll('MsgAdminAdminPlayer', '\c2The Admin made %2 an admin.', %client, %client.name);
      %client.isAdmin = 1;
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
	 messageAll('MsgAdminPlayer', '\c2%2 was made an admin by vote.', %client, %client.name);
	 %client.isAdmin = 1;
	 %cause = "(vote)";
      }
      else
	 messageAll('MsgVoteFailed', '\c2Vote to make %1 an admin did not pass.', %client.name);
   }
   if(%cause !$= "")
      logEcho(%client.nameBase@" (cl "@%client@") made admin "@%cause);
}

//------------------------------------------------------------------------------
function DefaultGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5)
{
   //the default behavior when clicking on a game link is to start observing that client
   %targetClient = %arg1;
   if ((%client.team == 0) && isObject(%targetClient) && (%targetClient.team != 0))
   {
      %prevObsClient = %client.observeClient;

      // update the observer list for this client
      observerFollowUpdate( %client, %targetClient, %prevObsClient !$= "" );

      serverCmdObserveClient(%client, %targetClient);
      displayObserverHud(%client, %targetClient);

      if (%targetClient != %prevObsClient)
      {
	 messageClient(%targetClient, 'Observer', '\c1%1 is now observing you.', %client.name);
	 messageClient(%prevObsClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
      }
   }
}

//------------------------------------------------------------------------------
$ScoreHudMaxVisible = 19;
function DefaultGame::updateScoreHud(%game, %client, %tag)
{
   if (Game.numTeams > 1)
   {
      // Send header:
      messageClient( %client, 'SetScoreHudHeader', "", '<tab:15,315>\t%1<rmargin:260><just:right>%2<rmargin:560><just:left>\t%3<just:right>%4',
	    %game.getTeamName(1), $TeamScore[1], %game.getTeamName(2), $TeamScore[2] );

      // Send subheader:
      messageClient( %client, 'SetScoreHudSubheader', "", '<tab:15,315>\tPLAYERS (%1)<rmargin:260><just:right>SCORE<rmargin:560><just:left>\tPLAYERS (%2)<just:right>SCORE',
	    $TeamRank[1, count], $TeamRank[2, count] );

      %index = 0;
      while ( true )
      {
	 if ( %index >= $TeamRank[1, count]+2 && %index >= $TeamRank[2, count]+2 )
	    break;

	 //get the team1 client info
	 %team1Client = "";
	 %team1ClientScore = "";
	 %col1Style = "";
	 if ( %index < $TeamRank[1, count] )
	 {
	    %team1Client = $TeamRank[1, %index];
	    %team1ClientScore = %team1Client.score $= "" ? 0 : %team1Client.score;
	    %col1Style = %team1Client == %client ? "<color:dcdcdc>" : "";
	    %team1playersTotalScore += %team1Client.score;
	 }
	 else if( %index == $teamRank[1, count] && $teamRank[1, count] != 0 && !isDemo() && %game.class $= "CTFGame")
	 {
	    %team1ClientScore = "--------------";
	 }
	 else if( %index == $teamRank[1, count]+1 && $teamRank[1, count] != 0 && !isDemo() && %game.class $= "CTFGame")
	 {
	    %team1ClientScore = %team1playersTotalScore != 0 ? %team1playersTotalScore : 0;
	 }
	 //get the team2 client info
	 %team2Client = "";
	 %team2ClientScore = "";
	 %col2Style = "";
	 if ( %index < $TeamRank[2, count] )
	 {
	    %team2Client = $TeamRank[2, %index];
	    %team2ClientScore = %team2Client.score $= "" ? 0 : %team2Client.score;
	    %col2Style = %team2Client == %client ? "<color:dcdcdc>" : "";
	    %team2playersTotalScore += %team2Client.score;
	 }
	 else if( %index == $teamRank[2, count] && $teamRank[2, count] != 0 && !isDemo() && %game.class $= "CTFGame")
	 {
	    %team2ClientScore = "--------------";
	 }
	 else if( %index == $teamRank[2, count]+1 && $teamRank[2, count] != 0 && !isDemo() && %game.class $= "CTFGame")
	 {
	    %team2ClientScore = %team2playersTotalScore != 0 ? %team2playersTotalScore : 0;
	 }

	 //if the client is not an observer, send the message
	 if (%client.team != 0)
	 {
	    messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20,320>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>%3</clip><just:right>%4',
		  %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style );
	 }
	 //else for observers, create an anchor around the player name so they can be observed
	 else
	 {
	    messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20,320>\t<spush>%5<clip:200><a:gamelink\t%7>%1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8>%3</a></clip><just:right>%4',
		  %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client );
	 }

	 %index++;
      }
   }
   else
   {
      //tricky stuff here...  use two columns if we have more than 15 clients...
      %numClients = $TeamRank[0, count];
      if ( %numClients > $ScoreHudMaxVisible )
	 %numColumns = 2;

      // Clear header:
      messageClient( %client, 'SetScoreHudHeader', "", "" );

      // Send header:
      if (%numColumns == 2)
	 messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,315>\tPLAYER<rmargin:270><just:right>SCORE<rmargin:570><just:left>\tPLAYER<just:right>SCORE');
      else
	 messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15>\tPLAYER<rmargin:270><just:right>SCORE');

      %countMax = %numClients;
      if ( %countMax > ( 2 * $ScoreHudMaxVisible ) )
      {
	 if ( %countMax & 1 )
	    %countMax++;
	 %countMax = %countMax / 2;
      }
      else if ( %countMax > $ScoreHudMaxVisible )
	 %countMax = $ScoreHudMaxVisible;

      for ( %index = 0; %index < %countMax; %index++ )
      {
	 //get the client info
	 %col1Client = $TeamRank[0, %index];
	 %col1ClientScore = %col1Client.score $= "" ? 0 : %col1Client.score;
	 %col1Style = %col1Client == %client ? "<color:dcdcdc>" : "";

	 //see if we have two columns
	 if ( %numColumns == 2 )
	 {
	    %col2Client = "";
	    %col2ClientScore = "";
	    %col2Style = "";

	    //get the column 2 client info
	    %col2Index = %index + %countMax;
	    if ( %col2Index < %numClients )
	    {
	       %col2Client = $TeamRank[0, %col2Index];
	       %col2ClientScore = %col2Client.score $= "" ? 0 : %col2Client.score;
	       %col2Style = %col2Client == %client ? "<color:dcdcdc>" : "";
	    }
	 }

	 //if the client is not an observer, send the message
	 if (%client.team != 0)
	 {
	    if ( %numColumns == 2 )
	       messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:25,325>\t<spush>%5<clip:195>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:195>%3</clip><just:right>%4',
		     %col1Client.name, %col1ClientScore, %col2Client.name, %col2ClientScore, %col1Style, %col2Style );
	    else
	       messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:25>\t%3<clip:195>%1</clip><rmargin:260><just:right>%2',
		     %col1Client.name, %col1ClientScore, %col1Style );
	 }
	 //else for observers, create an anchor around the player name so they can be observed
	 else
	 {
	    if ( %numColumns == 2 )
	       messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:25,325>\t<spush>%5<clip:195><a:gamelink\t%7>%1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:195><a:gamelink\t%8>%3</a></clip><just:right>%4',
		     %col1Client.name, %col1ClientScore, %col2Client.name, %col2ClientScore, %col1Style, %col2Style, %col1Client, %col2Client );
	    else
	       messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:25>\t%3<clip:195><a:gamelink\t%4>%1</a></clip><rmargin:260><just:right>%2',
		     %col1Client.name, %col1ClientScore, %col1Style, %col1Client );
	 }
      }

   }

   // Tack on the list of observers:
   %observerCount = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.team == 0)
	 %observerCount++;
   }

   if (%observerCount > 0)
   {
	   messageClient( %client, 'SetLineHud', "", %tag, %index, "");
      %index++;
		messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
      %index++;
      for (%i = 0; %i < ClientGroup.getCount(); %i++)
      {
	 %cl = ClientGroup.getObject(%i);
	 //if this is an observer
	 if (%cl.team == 0)
	 {
	    %obsTime = getSimTime() - %cl.observerStartTime;
	    %obsTimeStr = %game.formatTime(%obsTime, false);
		      messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150>%1</clip><rmargin:260><just:right>%2',
				     %cl.name, %obsTimeStr );
	    %index++;
	 }
      }
   }

   //clear the rest of Hud so we don't get old lines hanging around...
   messageClient( %client, 'ClearHud', "", %tag, %index );
}

//------------------------------------------------------------------------------
function UpdateClientTimes(%time)
{
    %secondsLeft = %time / 1000;
    messageAll('MsgSystemClock', "", (%secondsLeft / 60), %time);
}

//------------------------------------------------------------------------------
function notifyMatchStart(%time)
{
   %seconds = mFloor(%time / 1000);
   if (%seconds > 2)
      MessageAll('MsgMissionStart', '\c2Match starts in %1 seconds.~wfx/misc/hunters_%1.wav', %seconds);
   else if (%seconds == 2)
      MessageAll('MsgMissionStart', '\c2Match starts in 2 seconds.~wvoice/announcer/ann.match_begins.wav');
   else if (%seconds == 1)
      MessageAll('MsgMissionStart', '\c2Match starts in 1 second.');
   UpdateClientTimes(%time);
}

//------------------------------------------------------------------------------
function notifyMatchEnd(%time)
{
   %seconds = mFloor(%time / 1000);
   if (%seconds > 1)
      MessageAll('MsgMissionEnd', '\c2Match ends in %1 seconds.~wfx/misc/hunters_%1.wav', %seconds);
   else if (%seconds == 1)
      MessageAll('MsgMissionEnd', '\c2Match ends in 1 second.~wfx/misc/hunters_1.wav');
   UpdateClientTimes(%time);
}

function DefaultGame::formatTime(%game, %tStr, %includeHundredths)
{
   %timeInSeconds = %tStr / 1000;
   %mins = mFloor(%timeInSeconds / 60);
   if(%mins < 1)
      %timeString = "00:";
   else if(%mins < 10)
      %timeString = "0" @ %mins @ ":";
   else
      %timeString = %mins @ ":";

   %timeInSeconds -= (%mins * 60);
   %secs = mFloor(%timeInSeconds);
   if(%secs < 1)
      %timeString = %timeString @ "00";
   else if(%secs < 10)
      %timeString = %timeString @ "0" @ %secs;
   else
      %timeString = %timeString @ %secs;

   if (%includeHundredths)
   {
      %timeString = %timeString @ ".";
      %timeInSeconds -= %secs;
      %hSecs = mFloor(%timeInSeconds * 100); // will be between 0 and 999
      if(%hSecs < 1)
	 %timeString = %timeString @ "00";
      else if(%hSecs < 10)
	 %timeString = %timeString @ "0" @ %hSecs;
      else
	 %timeString = %timeString @ %hSecs;
   }

   return %timeString;
}

//------------------------------------------------------------------------------
//AI FUNCTIONS
function DefaultGame::AIChooseGameObjective(%game, %client)
{
   AIChooseObjective(%client);
}

//------------------------------------------------------------------------------
function DefaultGame::getServerStatusString(%game)
{
   %status = %game.numTeams;
   for ( %team = 1; %team - 1 < %game.numTeams; %team++ )
   {
      %score = isObject( $teamScore[%team] ) ? $teamScore[%team] : 0;
      %teamStr = getTaggedString( %game.getTeamName(%team) ) TAB %score;
      %status = %status NL %teamStr;
   }

   %status = %status NL ClientGroup.getCount();
   for ( %i = 0; %i < ClientGroup.getCount(); %i++ )
   {
      %cl = ClientGroup.getObject( %i );
      %score = %cl.score $= "" ? 0 : %cl.score;
      %playerStr = getTaggedString( %cl.name ) TAB getTaggedString( %game.getTeamName(%cl.team) ) TAB %score;
      %status = %status NL %playerStr;
   }
   return( %status );
}

//------------------------------------------------------------------------------
function DefaultGame::OptionsDlgSleep( %game )
{
   // ignore in the default game...
}

//------------------------------------------------------------------------------
function DefaultGame::endMission( %game )
{
}
