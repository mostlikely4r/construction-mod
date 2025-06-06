//******************************************************************************
//*   Station  -  Data Blocks                                                  *
//******************************************************************************
datablock EffectProfile(StationInventoryActivateEffect)
{
   effectname = "powered/inv_pad_on";
   minDistance = 5.0;
   maxDistance = 7.5;
};

datablock EffectProfile(StationVehicleAcitvateEffect)
{
   effectname = "powered/vehicle_screen_on2";
   minDistance = 3.0;
   maxDistance = 5.0;
};

datablock EffectProfile(StationVehicleDeactivateEffect)
{
   effectname = "powered/vehicle_screen_off";
   minDistance = 3.0;
   maxDistance = 5.0;
};

//datablock EffectProfile(MobileBaseInventoryActivateEffect)
//{
//   effectname = "misc/diagnostic_on";
//   minDistance = 3.0;
//};

datablock EffectProfile(StationAccessDeniedEffect)
{
   effectname = "powered/station_denied";
   minDistance = 3.0;
   maxDistance = 5.0;
};

datablock AudioProfile(StationInventoryActivateSound)
{
   filename = 	"fx/powered/inv_pad_on.wav";
   description = AudioClose3d;
   preload = 	 true;
   effect = StationInventoryActivateEffect;
};

datablock AudioProfile(MobileBaseInventoryActivateSound)
{
   filename    = "fx/vehicles/mpb_inv_station.wav";
   description = AudioClose3d;
   preload = true;
   effect = StationInventoryActivateEffect;
};

datablock AudioProfile(DepInvActivateSound)
{
   filename = 	"fx/powered/dep_inv_station.wav";
   description = AudioClose3d;
   preload = 	 true;
   effect = StationInventoryActivateEffect;
};

datablock AudioProfile(StationVehicleAcitvateSound)
{
   filename    = "fx/powered/vehicle_screen_on2.wav";
   description = AudioClosest3d;
   preload = true;
   effect = StationVehicleAcitvateEffect;
};

datablock AudioProfile(StationVehicleDeactivateSound)
{
   filename    = "fx/powered/vehicle_screen_off.wav";
   description = AudioClose3d;
   preload = true;
   effect = StationVehicleDeactivateEffect;
};

datablock AudioProfile(StationAccessDeniedSound)
{
   filename    = "fx/powered/station_denied.wav";
   description = AudioClosest3d;
   preload = true;
   effect = StationAccessDeniedEffect;
};

datablock AudioProfile(StationVehicleHumSound)
{
   filename    = "fx/powered/station_hum.wav";
   description = CloseLooping3d;
   preload = true;
};

datablock AudioProfile(StationInventoryHumSound)
{
   filename    = "fx/powered/station_hum.wav";
   description = CloseLooping3d;
   preload = true;
};

datablock StationFXPersonalData( PersonalInvFX )
{
   delay = 0;
   fadeDelay = 0.5;
   lifetime = 1.2;
   height = 2.5;
   numArcSegments = 10.0;
   numDegrees = 180.0;
   trailFadeTime = 0.5;
   leftRadius = 1.85;
   rightRadius = 1.85;
   leftNodeName = "FX1";
   rightNodeName = "FX2";

   texture[0] = "special/stationLight";
};


datablock DebrisData( StationDebris )
{
   explodeOnMaxBounce = false;

   elasticity = 0.40;
   friction = 0.5;

   lifetime = 17.0;
   lifetimeVariance = 0.0;

   minSpinSpeed = 60;
   maxSpinSpeed = 600;

   numBounces = 10;
   bounceVariance = 0;

   staticOnMaxBounce = true;

   useRadiusMass = true;
   baseRadius = 0.4;

   velocity = 9.0;
   velocityVariance = 4.5;

};

datablock StaticShapeData(StationInventory) : StaticShapeDamageProfile
{
   className = Station;
   catagory = "Stations";
   shapeFile = "station_inv_human.dts";
   maxDamage = 1.00;
   destroyedLevel = 1.00;
   disabledLevel = 0.70;
   explosion      = ShapeExplosion;
	expDmgRadius = 8.0;
	expDamage = 0.4;
	expImpulse = 1500.0;
	// don't allow this object to be damaged in non-team-based
	// mission types (DM, Rabbit, Bounty, Hunters)
	noIndividualDamage = true;

   dynamicType = $TypeMasks::StationObjectType;
	isShielded = true;
	energyPerDamagePoint = 75;
	maxEnergy = 50;
	rechargeRate = 0.35;
   doesRepair = true;
   humSound = StationInventoryHumSound;

   cmdCategory = "Support";
   cmdIcon = CMDStationIcon;
   cmdMiniIconName = "commander/MiniIcons/com_inventory_grey";
   targetNameTag = 'Inventory';
   targetTypeTag = 'Station';

   debrisShapeName = "debris_generic.dts";
   debris = StationDebris;
	needsPower = true;
};

datablock StaticShapeData(StationAmmo) : StaticShapeDamageProfile
{
   className = Station;
   catagory = "Stations";
//   shapeFile = "station_ammo.dts";
   shapeFile = "station_inv_human.dts";
   maxDamage = 1.00;
   destroyedLevel = 1.00;
   disabledLevel = 0.70;
   explosion      = ShapeExplosion;
	expDmgRadius = 8.0;
	expDamage = 0.4;
	expImpulse = 1500.0;
	// don't allow this object to be damaged in non-team-based
	// mission types (DM, Rabbit, Bounty, Hunters)
	noIndividualDamage = true;

   dynamicType = $TypeMasks::StationObjectType;
	isShielded = true;
	energyPerDamagePoint = 75;
	maxEnergy = 50;
	rechargeRate = 0.35;
   doesRepair = true;
   humSound = StationInventoryHumSound;

   cmdCategory = "Support";
   cmdIcon = CMDStationIcon;
   cmdMiniIconName = "commander/MiniIcons/com_inventory_grey";
   targetNameTag = 'Ammo';
   targetTypeTag = 'Station';

   debrisShapeName = "debris_generic.dts";
   debris = StationDebris;
};

datablock StaticShapeData(StationVehicle) : StaticShapeDamageProfile
{
//[[CHANGE]] Added some observer information..
   className = Station;
   catagory = "Stations";
   shapeFile = "vehicle_pad_station.dts";
   maxDamage = 1.20;
   destroyedLevel = 1.20;
   disabledLevel = 0.84;
   explosion      = ShapeExplosion;
   canControl = true;
	expDmgRadius = 10.0;
	expDamage = 0.4;
	expImpulse = 1500.0;
   dynamicType = $TypeMasks::StationObjectType;
	isShielded = true;
	energyPerDamagePoint = 33;
	maxEnergy = 250;
	rechargeRate = 0.31;
   humSound = StationVehicleHumSound;
	// don't let these be damaged in Siege missions
	noDamageInSiege = true;
   cmdCategory = "Support";
   cmdIcon = CMDVehicleStationIcon;
   cmdMiniIconName = "commander/MiniIcons/com_vehicle_pad_inventory";
   targetTypeTag = 'Vehicle Station';
   observeParameters = "0 10.0 10.0";

   debrisShapeName = "debris_generic.dts";
   debris = StationDebris;

   minLookAngle = -1.5;
   maxLookAngle = 1.5;
   maxFreelookAngle = 3.0;

   cameraMaxDist = 40;
   cameraOffset = 20;
   cameraLag = 1.5;
	noIndividualDamage = true;
};

datablock StaticShapeData(StationVehiclePad)
{
   className = Station;
   catagory = "Stations";
   shapeFile = "vehicle_pad.dts";
   isInvincible = true;
   dynamicType = $TypeMasks::StaticObjectType;
   rechargeRate = 0.05;

   // ------------------------------------------
   // z0dd - ZOD, 5/8/02. CC bug fix parameter.
   // Part of overall V-Station creation change.
   targetTypeTag = 'Vehicle Pad';
};

//datablock StaticShapeData(StationAmmo)
//{
//   className = Station;
//   catagory = "Stations";
//   shapeFile = "station_ammo.dts";
//   maxDamage = 1.0;
//   disabledLevel = 0.6;
//   destroyedLevel = 0.8;
//   icon = "CMDStationIcon";
//   dynamicType = $TypeMasks::StationObjectType;
//};

datablock StaticShapeData(MobileInvStation)
{
   className = Station;
   catagory = "Stations";
   shapeFile = "station_inv_mpb.dts";
   icon = "CMDStationIcon";
	// don't allow this object to be damaged in non-team-based
	// mission types (DM, Rabbit, Bounty, Hunters)
	noIndividualDamage = true;

   dynamicType = $TypeMasks::StationObjectType;
	rechargeRate = 0.256;
   doesRepair = true;
};


//******************************************************************************
//*   Station  -  Functions                                                    *
//******************************************************************************

////////////////////////////////////////////////////////////////////////////////
/// -Inventory- ////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- onAdd (%this, %obj)
//                %this = Object data block
//                %obj = Object being added
//Decription -- Called when the object is added to the mission
////////////////////////////////////////////////////////////////////////////////
function StationInventory::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);

   %obj.setRechargeRate(%obj.getDatablock().rechargeRate);
   %trigger = new Trigger()
   {
      dataBlock = stationTrigger;
      polyhedron = "-0.75 0.75 0.1 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.3";
   };
   MissionCleanup.add(%trigger);
   %trigger.setTransform(%obj.getTransform());

   %trigger.station = %obj;
   %trigger.mainObj = %obj;
   %trigger.disableObj = %obj;
   %obj.trigger = %trigger;
}

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- stationReady(%data, %obj)
//                %data = Station Data Block
//                %obj = Station Object
//Decription -- Called when station has been triggered and animation is
//              completed
////////////////////////////////////////////////////////////////////////////////
function StationInventory::stationReady(%data, %obj)
{
   //Display the Inventory Station GUI here
   %obj.notReady = 1;
   %obj.inUse = "Down";
   %obj.schedule(500,"playThread",$ActivateThread,"activate1");
   %player = %obj.triggeredBy;
   %energy = %player.getEnergyLevel();
   // -------------------------------------------------
   // z0dd - ZOD, 4/20/02. Addition. Inv energy bug fix
   %max = %player.getDatablock().maxEnergy;

   %player.setCloaked(true);
   %player.schedule(500, "setCloaked", false);
	if (!%player.client.isAIControlled())
	   buyFavorites(%player.client);

   // -------------------------------------------------
   // z0dd - ZOD, 4/20/02. Addition. Inv energy bug fix
   //%player.setEnergyLevel(%energy);
   %player.setEnergyLevel(mFloor(%player.getDatablock().maxEnergy * %energy / %max));

   %data.schedule( 500, "beginPersonalInvEffect", %obj );
}

function StationInventory::beginPersonalInvEffect( %data, %obj )
{
   if (!%obj.isDisabled())
   {
      %fx = new StationFXPersonal()
      {
         dataBlock = PersonalInvFX;
         stationObject    = %obj;
      };
   }
}

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- stationFinished(%data, %obj)
//                %data = Station Data Block
//                %obj = Station Object
//Decription -- Called when player has left the station
////////////////////////////////////////////////////////////////////////////////
function StationInventory::stationFinished(%data, %obj)
{
   //Hide the Inventory Station GUI
}

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate
//              animation.
////////////////////////////////////////////////////////////////////////////////
function StationInventory::getSound(%data, %forward)
{
   if(%forward)
      return "StationInventoryActivateSound";
   else
      return false;
}

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- setPlayerPosition(%data, %obj, %trigger, %colObj)
//                %data = Station Data Block
//                %obj = Station Object
//                %trigger = Stations trigger
//                %colObj = Object that is at the station
//Decription -- Called when player enters the trigger.  Used to set the player
//              in the center of the station.
////////////////////////////////////////////////////////////////////////////////
function StationInventory::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      %pos = %trigger.position;
      %colObj.setvelocity("0 0 0");
	   %rot = getWords(%colObj.getTransform(),3, 6);
	%pos = getWord(%pos,0) @ " " @ getWord(%pos,1) @ " " @ getWord(%pos,2) + 0.8;
	%terrain = getTerrainHeight2(%pos);
	if (!(getWord(%pos,2) < getWord(%terrain,2) || %terrain $= "") || $Host::AllowUnderground)
		//%colObj.setTransform(%pos @ " " @ %rot);//center player on object
      return true;
   }
   return false;
}

///////////////////////////////////////////////////////////////////////////////
/// -Ammo- ////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

function StationAmmo::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);

   %obj.setRechargeRate(%obj.getDatablock().rechargeRate);
   %trigger = new Trigger()
   {
      dataBlock = stationTrigger;
      polyhedron = "-0.75 0.75 0.1 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.3";
   };
   MissionCleanup.add(%trigger);
   %trigger.setTransform(%obj.getTransform());

   %trigger.station = %obj;
   %trigger.mainObj = %obj;
   %trigger.disableObj = %obj;
   %obj.trigger = %trigger;
}

//-------------------------------------------------------------------------------
function StationAmmo::stationReady(%data, %obj)
{
    //error("StationAmmo::stationReady");
    %obj.notReady = 1;
    %obj.inUse = "Down";
    %obj.setThreadDir($ActivateThread, true);
    %obj.schedule(100, "playThread", $ActivateThread, "activate1");
    %player = %obj.triggeredBy;
    %energy = %player.getEnergyLevel();
    //%player.setCloaked(true);
    //%player.schedule(500, "setCloaked", false);


    if (!%player.client.isAIControlled())
        getAmmoStationLovin(%player.client);
    //%data.schedule( 500, "beginPersonalInvEffect", %obj );
}

//-------------------------------------------------------------------------------
function StationAmmo::onEndSequence(%data, %obj, %thread)
{
    if(%thread == $ActivateThread)
    {
        %obj.setThreadDir($ActivateThread, false);
        %obj.playThread( $ActivateThread, "activate1");
        if(%obj.inUse $= "Up")
        {
            %data.stationReady(%obj);
            %player = %obj.triggeredBy;
            if(%data.doesRepair && !%player.stationRepairing && %player.getDamageLevel() != 0) {
                %oldRate = %player.getRepairRate();
                %player.setRepairRate(%oldRate + 0.00625);
                %player.stationRepairing = 1;
            }
        }
    }
   //Parent::onEndSequence(%data, %obj, %thread);
}

//-------------------------------------------------------------------------------
function StationAmmo::stationFinished(%data, %obj)
{
   //Hide the Inventory Station GUI
}

//-------------------------------------------------------------------------------
function StationAmmo::getSound(%data, %forward)
{
   if(%forward)
      return "StationInventoryActivateSound";
   else
      return false;
}

//-------------------------------------------------------------------------------
function StationAmmo::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      %pos = %trigger.position;
      %colObj.setvelocity("0 0 0");
	   %rot = getWords(%colObj.getTransform(),3, 6);
	%pos = getWord(%pos,0) @ " " @ getWord(%pos,1) @ " " @ getWord(%pos,2) + 0.8;
	%terrain = getTerrainHeight2(%pos);
	if (!(getWord(%pos,2) < getWord(%terrain,2) || %terrain $= "") || $Host::AllowUnderground)
		//%colObj.setTransform(%pos @ " " @ %rot);//center player on object
      return true;
   }
   return false;
}

////////////////////////////////////////////////////////////////////////////////
/// -Vehicle- //////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- onAdd (%this, %obj)
//                %this = Object data block
//                %obj = Object being added
//Decription -- Called when the object is added to the mission
////////////////////////////////////////////////////////////////////////////////

// -----------------------------------------------------------------------------
// z0dd - ZOD - Founder(founder@mechina.com), 5/8/02. Totall re-write of Vehicle 
// station creation. More stable, addresses some power related bugs.

// Do not need this anymore.
//function StationVehicle::onAdd(%this, %obj)
//{
//   Parent::onAdd(%this, %obj);

//	%obj.setRechargeRate(%obj.getDatablock().rechargeRate);
//   %trigger = new Trigger()
//   {
//      dataBlock = stationTrigger;
//      polyhedron = "-0.75 0.75 0.0 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.0";
//   };             
//   MissionCleanup.add(%trigger);
//   %trigger.setTransform(%obj.getTransform());
//   %trigger.station = %obj;
//   %obj.trigger = %trigger;
//}

// This now creates the vehicle station trigger.
function StationVehicle::createTrigger(%this, %obj)
{
   %trigger = new Trigger()
   {
      dataBlock = stationTrigger;
      polyhedron = "-0.75 0.75 0.0 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.0";
   };             
   MissionCleanup.add(%trigger);
   %trigger.setTransform(%obj.getTransform());
   %trigger.station = %obj;
   %obj.trigger = %trigger;
}
// End z0dd - ZOD - Founder
// -----------------------------------------------------------------------------

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- stationReady(%data, %obj)
//                %data = Station Data Block
//                %obj = Station Object
//Decription -- Called when station has been triggered and animation is
//              completed
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::stationReady(%data, %obj)
{
   // Make sure none of the other popup huds are active:
   messageClient( %obj.triggeredBy.client, 'CloseHud', "", 'scoreScreen' );
   messageClient( %obj.triggeredBy.client, 'CloseHud', "", 'inventoryScreen' );

   //[[CHANGE]] Make sure the player teleports...
   %obj.triggeredby.client.telebuy = "";

   //Display the Vehicle Station GUI
   commandToClient(%obj.triggeredBy.client, 'StationVehicleShowHud');
}

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- stationFinished(%data, %obj)
//                %data = Station Data Block
//                %obj = Station Object
//Decription -- Called when player has left the station
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::stationFinished(%data, %obj)
{
   //Hide the Vehicle Station GUI
   if(!%obj.triggeredBy.isMounted())
      commandToClient(%obj.triggeredBy.client, 'StationVehicleHideHud');
   else
      commandToClient(%obj.triggeredBy.client, 'StationVehicleHideJustHud');

}

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate
//              animation.
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::getSound(%data, %forward)
{
   if(%forward)
      return "StationVehicleAcitvateSound";
   else
      return "StationVehicleDeactivateSound";
}

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- setPlayerPosition(%data, %obj, %trigger, %colObj)
//                %data = Station Data Block
//                %obj = Station Object
//                %trigger = Stations trigger
//                %colObj = Object that is at the station
//Decription -- Called when player enters the trigger.  Used to set the player
//              in the center of the station.
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      %posXY = getWords(%trigger.getTransform(),0 ,1);
      %posZ = getWord(%trigger.getTransform(), 2);
      %rotZ =  getWord(%obj.getTransform(), 5);
      %angle =  getWord(%obj.getTransform(), 6);
	   %angle += 3.141592654;
      if(%angle > 6.283185308)
         %angle = %angle - 6.283185308;
      %colObj.setvelocity("0 0 0");
	%pos = %posXY @ " " @ %posZ + 0.2;
	%terrain = getTerrainHeight2(%pos);
	if (!(getWord(%pos,2) < getWord(%terrain,2) || %terrain $= "") || $Host::AllowUnderground)
		%colObj.setTransform(%pos @ " " @ "0 0 "  @ %rotZ @ " " @ %angle );//center player on object
      return true;
   }
   return false;
}
// -----------------------------------------------------------------------------
// z0dd - ZOD - Founder(founder@mechina.com), 5/8/02. Totall re-write of Vehicle 
// station creation. More stable, addresses some power related bugs.

function StationVehiclePad::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);

	%obj.ready = true;
   %obj.setRechargeRate(%obj.getDatablock().rechargeRate);

   //-------------------------------------------------------------
   // All of this moved to StationVehiclePad::createStationVehicle
   //%xform = %obj.getSlotTransform(0);
   //%pos = getWords(%xform, 0, 2);
   //%rot = getWords(%xform, 3, 5);
   //%angle = (getWord(%xform, 6) * 180) / 3.14159;

   //%sv = new StaticShape() {
   //   scale = "1 1 1";
   //   dataBlock = StationVehicle;
   //   lockCount = "0";
   //   homingCount = "0";
   //   team = %obj.team;
   //   position = %pos;
   //   rotation = %rot @ " " @ %angle;
   //};
   
   //MissionCleanup.add(%sv);
   //%sv.getDataBlock().gainPower(%sv);
   //%sv.pad = %obj;
   //%obj.station = %sv;
   //%sv.trigger.mainObj = %obj;
   //%sv.trigger.disableObj = %sv;

   //Remove unwanted vehicles
   //if(%obj.scoutVehicle !$= "Removed")
   //   %sv.vehicle[scoutvehicle] = true;
   //if(%obj.assaultVehicle !$= "Removed")
   //   %sv.vehicle[assaultVehicle] = true;
   //if(%obj.mobileBaseVehicle !$= "Removed")
   //{
   //   %sv.vehicle[mobileBasevehicle] = true;
   //}
   //if(%obj.scoutFlyer !$= "Removed")
   //   %sv.vehicle[scoutFlyer] = true;
   //if(%obj.bomberFlyer !$= "Removed")
   //   %sv.vehicle[bomberFlyer] = true;
   //if(%obj.hapcFlyer !$= "Removed")
   //   %sv.vehicle[hapcFlyer] = true;

   // z0dd - ZOD - Founder. Schedule the vehicle station creation.
   // Don't create the station if the pad is hidden by the current mission type.
   //error("CURRENT MISSION TYPE: " @ $CurrentMissionType @ ", ALLOWED TYPE: " @ %obj.missionTypesList);
   if($CurrentMissionType $= %obj.missionTypesList || %obj.missionTypesList $="")
      %this.schedule(0, "createStationVehicle", %obj);
}

function StationVehiclePad::createStationVehicle(%data, %obj)
{
   // This code used to be called from StationVehiclePad::onAdd
   // This was changed so we can add the station to the mission group
   // so it gets powered properly and auto cleaned up at mission end.

   // Get the v-pads mission group so we can place the station in it.
   %group = %obj.getGroup();

   // Set the default transform based on the vehicle pads slot
   %xform = %obj.getSlotTransform(0);
   %position = getWords(%xform, 0, 2);
   %rotation = getWords(%xform, 3, 5);
   %angle = (getWord(%xform, 6) * 180) / 3.14159;

   // Place these parameter's in the v-pad datablock located in mis file.
   // If the mapper doesn't move the station, use the default location.
   if(%obj.stationPos $= "" || %obj.stationRot $= "")
   {
      %pos = %position;
      %rot = %rotation @ " " @ %angle;
   }
   else
   {
      %pos = %obj.stationPos;
      %rot = %obj.stationRot;
   }

   %sv = new StaticShape() {
	scale = "1 1 1";
      dataBlock = "StationVehicle";
	lockCount = "0";
	homingCount = "0";
	team = %obj.team;
      position = %pos;
      rotation = %rot;
   };

   // Add the station to the v-pads mission group for cleanup and power.
   %group.add(%sv);
   %sv.setPersistent(false); // set the station to not save.

   // Apparently called to early on mission load done, call it now.
   %sv.getDataBlock().gainPower(%sv);

   // Create the trigger
   %sv.getDataBlock().createTrigger(%sv);
   %sv.pad = %obj;
   %obj.station = %sv;
   %sv.trigger.mainObj = %obj;
   %sv.trigger.disableObj = %sv;

   // Set the sensor group.
   if(%sv.getTarget() != -1)
      setTargetSensorGroup(%sv.getTarget(), %obj.team);

   //Remove unwanted vehicles
   if(%obj.scoutVehicle !$= "Removed")
	%sv.vehicle[scoutvehicle] = true;
   if(%obj.SuperScoutVehicle !$= "Removed")
	%sv.vehicle[SuperScoutVehicle] = true;
   if(%obj.assaultVehicle !$= "Removed")
	%sv.vehicle[assaultVehicle] = true;
   if(%obj.mobileBaseVehicle !$= "Removed")
	%sv.vehicle[mobileBasevehicle] = true;
   if(%obj.scoutFlyer !$= "Removed")
	%sv.vehicle[scoutFlyer] = true;
   if(%obj.bomberFlyer !$= "Removed")
	%sv.vehicle[bomberFlyer] = true;
   if(%obj.hapcFlyer !$= "Removed")
	%sv.vehicle[hapcFlyer] = true;
   if(%obj.SuperHAPCFlyer !$= "Removed")
	%sv.vehicle[SuperHAPCFlyer] = true;
   if(%obj.Artillery !$= "Removed")
	%sv.vehicle[Artillery] = true;
}
// End z0dd - ZOD - Founder
// -----------------------------------------------------------------------------

function StationVehiclePad::onEndSequence(%data, %obj, %thread)
{
   if(%thread == $ActivateThread)
   {
      %obj.ready = true;
      %obj.stopThread($ActivateThread);
   }
   Parent::onEndSequence(%data, %obj, %thread);
}

////////////////////////////////////////////////////////////////////////////////
/// -Mobile Base Inventory- ////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- onAdd (%this, %obj)
//                %this = Object data block
//                %obj = Object being added
//Decription -- Called when the object is added to the mission
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::onAdd(%this, %obj)
{
}

function MobileInvStation::createTrigger(%this, %obj)
{
   Parent::onAdd(%this, %obj);

	%obj.setRechargeRate(%obj.getDatablock().rechargeRate);
   %trigger = new Trigger()
   {
      dataBlock = stationTrigger;
      polyhedron = "-0.75 0.75 0.1 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.3";
   };
   MissionCleanup.add(%trigger);
   %trigger.setTransform(%obj.vehicle.getSlotTransform(2));

   %trigger.station = %obj;
   %trigger.mainObj = %obj;
   %trigger.disableObj = %obj;

   %obj.trigger = %trigger;
//   createTarget(%obj, 'Inventory Station', "", "", 'Station', 0, 0);
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- stationReady(%data, %obj)
//                %data = Station Data Block
//                %obj = Station Object
//Decription -- Called when station has been triggered and animation is
//              completed
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::stationReady(%data, %obj)
{
   //Display the Inventory Station GUI here
   %obj.notReady = 1;
   %obj.inUse = "Down";
   %obj.schedule(200,"playThread",$ActivateThread,"activate1");
   %obj.getObjectMount().playThread($ActivateThread,"Activate");
   %player = %obj.triggeredBy;
   %energy = %player.getEnergyLevel();
   %player.setCloaked(true);
   %player.schedule(900, "setCloaked", false);
	if (!%player.client.isAIControlled())
	   buyFavorites(%player.client);

   %player.setEnergyLevel(%energy);
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- stationFinished(%data, %obj)
//                %data = Station Data Block
//                %obj = Station Object
//Decription -- Called when player has left the station
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::stationFinished(%data, %obj)
{
   //Hide the Inventory Station GUI
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate
//              animation.
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::getSound(%data, %forward)
{
   if(%forward)
      return "MobileBaseInventoryActivateSound";
   else
      return false;
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- setPlayerPosition(%data, %obj, %trigger, %colObj)
//                %data = Station Data Block
//                %obj = Station Object
//                %trigger = Stations trigger
//                %colObj = Object that is at the station
//Decription -- Called when player enters the trigger.  Used to set the player
//              in the center of the station.
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      %pos = %trigger.position;
      %colObj.setvelocity("0 0 0");
	   %rot = getWords(%colObj.getTransform(),3, 6);
	%pos = getWord(%pos,0) @ " " @ getWord(%pos,1) @ " " @ getWord(%pos,2)+0.8;
	%terrain = getTerrainHeight2(%pos);
	if (!(getWord(%pos,2) < getWord(%terrain,2) || %terrain $= "") || $Host::AllowUnderground)
		//%colObj.setTransform(%pos @ " " @ %rot);//center player on object
      return true;
   }
   return false;
}

function MobileInvStation::onDamage()
{
}

function MobileInvStation::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
   //If vehicle station is hit then apply damage to the vehicle
   %targetObject.getObjectMount().damage(%sourceObject, %position, %amount, %damageType);
}

////////////////////////////////////////////////////////////////////////////////
/// -Station Trigger- //////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

////-Station Trigger-///////////////////////////////////////////////////////////
//Function -- onEnterTrigger (%data, %obj, %colObj)
//                %data = Trigger Data Block
//                %obj = Trigger Object
//                %colObj = Object that collided with the trigger
//Decription -- Called when trigger has been triggered
////////////////////////////////////////////////////////////////////////////////
function stationTrigger::onEnterTrigger(%data, %obj, %colObj)
{
	//make sure it's a player object, and that that object is still alive
   if(%colObj.getDataBlock().className !$= "Armor" || %colObj.getState() $= "Dead")
      return;

   %colObj.inStation = true;
   commandToClient(%colObj.client,'setStationKeys', true);
   if(Game.stationOnEnterTrigger(%data, %obj, %colObj)) {
      //verify station.team is team associated and isn't on player's team
      if((%obj.mainObj.team != %colObj.client.team) && (%obj.mainObj.team != 0))
      {
         //%obj.station.playAudio(2, StationAccessDeniedSound);
         messageClient(%colObj.client, 'msgStationDenied', '\c2Access Denied -- Wrong team.~wfx/powered/station_denied.wav');
      }
      else if(%obj.disableObj.isDisabled())
      {
         messageClient(%colObj.client, 'msgStationDisabled', '\c2Station is disabled.');
      }
      else if(!%obj.mainObj.isPowered())
      {
         messageClient(%colObj.client, 'msgStationNoPower', '\c2Station is not powered.');
      }
      else if(%obj.station.notDeployed)
      {
         messageClient(%colObj.client, 'msgStationNotDeployed', '\c2Station is not deployed.');
      }
      else if(%obj.station.triggeredBy $= "")
      {
         if(%obj.station.getDataBlock().setPlayersPosition(%obj.station, %obj, %colObj))
         {
            messageClient(%colObj.client, 'CloseHud', "", 'inventoryScreen');
            commandToClient(%colObj.client, 'TogglePlayHuds', true);
            %obj.station.triggeredBy = %colObj;
            %obj.station.getDataBlock().stationTriggered(%obj.station, 1);
            %colObj.station = %obj.station;
            %colObj.lastWeapon = ( %colObj.getMountedImage($WeaponSlot) == 0 ) ? "" : %colObj.getMountedImage($WeaponSlot).getName().item;
            %colObj.unmountImage($WeaponSlot);
         }
      }
   }
}


////-Station Trigger-///////////////////////////////////////////////////////////
//Function -- onLeaveTrigger (%data, %obj, %colObj)
//                %data = Trigger Data Block
//                %obj = Trigger Object
//                %colObj = Object that collided with the trigger
//Decription -- Called when trigger has been untriggered
////////////////////////////////////////////////////////////////////////////////
function stationTrigger::onLeaveTrigger(%data, %obj, %colObj)
{
   if(%colObj.getDataBlock().className !$= "Armor")
      return;

   %colObj.inStation = false;
   commandToClient(%colObj.client,'setStationKeys', false);
   if(%obj.station)
   {
      if(%obj.station.triggeredBy == %colObj)
      {
         %obj.station.getDataBlock().stationFinished(%obj.station);
         %obj.station.getDataBlock().endRepairing(%obj.station);
         %obj.station.triggeredBy = "";
         %obj.station.getDataBlock().stationTriggered(%obj.station, 0);

         if(!%colObj.teleporting)
            %colObj.station = "";
	if(%colObj.getMountedImage($WeaponSlot) == 0 && !%colObj.teleporting)
	      {
	         if(%colObj.inv[%colObj.lastWeapon])
	            %colObj.use(%colObj.lastWeapon);

            if(%colObj.getMountedImage($WeaponSlot) == 0)
               %colObj.selectWeaponSlot( 0 );
	      }
      }
   }
}

////-Station Trigger-///////////////////////////////////////////////////////////
//Function -- stationTriggered(%data, %obj, %isTriggered)
//                %data = Station Data Block
//                %obj = Station Object
//                %isTriggered = 1 if triggered; 0 if status changed to
//                               untriggered
//Decription -- Called when a "station trigger" has been triggered or
//              untriggered
////////////////////////////////////////////////////////////////////////////////
function Station::stationTriggered(%data, %obj, %isTriggered)
{


   if(%isTriggered)
   {
      %obj.setThreadDir($ActivateThread, TRUE);
      %obj.playThread($ActivateThread,"activate");
      %obj.playAudio($ActivateSound, %data.getSound(true));
      %obj.inUse = "Up";
   }
   else
   {
      if(%obj.getDataBlock().getName() !$= StationVehicle)
      {
         %obj.stopThread($ActivateThread);
         if(%obj.getObjectMount())
            %obj.getObjectMount().stopThread($ActivateThread);
         %obj.inUse = "Down";
      }
      else
      {
         %obj.setThreadDir($ActivateThread, FALSE);
         %obj.playThread($ActivateThread,"activate");
         %obj.playAudio($ActivateSound, %data.getSound(false));
         %obj.inUse = "Down";
      }
   }
}

////-Station-///////////////////////////////////////////////////////////////////
//Function -- onEndSequence(%data, %obj, %thread)
//                %data = Station Data Block
//                %obj = Station Object
//                %thread = Thread number that the animation is associated
//                          with / running on.
//Decription -- Called when an animation sequence is finished playing
////////////////////////////////////////////////////////////////////////////////
function Station::onEndSequence(%data, %obj, %thread)
{
   if(%thread == $ActivateThread)
   {
      if(%obj.inUse $= "Up")
      {
         %data.stationReady(%obj);
         %player = %obj.triggeredBy;
	      if(%data.doesRepair && !%player.stationRepairing && %player.getDamageLevel() != 0) {
	         %oldRate = %player.getRepairRate();
	         %player.setRepairRate(%oldRate + 0.00625);
	         %player.stationRepairing = 1;
	      }
      }
      else
      {
         if(%obj.getDataBlock().getName() !$= MobileInvStation)
         {
            %obj.stopThread($ActivateThread);
            %obj.inUse = "Down";
         }
      }
   }
   Parent::onEndSequence(%data, %obj, %thread);
}

////-Station-///////////////////////////////////////////////////////////////////
//Function -- onCollision(%data, %obj, %colObj)
//                %data = Station Data Block
//                %obj = Station Object
//                %colObj = Object that collided with the station
//Decription -- Called when an object collides with a station
////////////////////////////////////////////////////////////////////////////////
function Station::onCollision(%data, %obj, %colObj)
{
	// Currently Not Needed
}

////-Station-///////////////////////////////////////////////////////////////////
//Function -- endRepairing(%data, %obj)
//                %data = Station Data Block
//                %obj = Station Object
//Decription -- Called to stop repairing the object
////////////////////////////////////////////////////////////////////////////////
function Station::endRepairing(%data, %obj)
{
   if(%obj.triggeredBy.stationRepairing)
   {
      %oldRate = %obj.triggeredBy.getRepairRate();
      %obj.triggeredBy.setRepairRate(%oldRate - 0.00625);
      %obj.triggeredBy.stationRepairing = 0;
   }
}

////-Station Trigger-///////////////////////////////////////////////////////////
//Function -- onTickTrigger(%data, %obj)
//                %data = Trigger Data Block
//                %obj = Trigger Object
//Decription -- Called every tick if triggered
////////////////////////////////////////////////////////////////////////////////
function stationTrigger::onTickTrigger(%data, %obj)
{
}

//******************************************************************************
//*   Station General  -  Functions                                            *
//******************************************************************************

//function Station::onGainPowerEnabled(%data, %obj)

function Station::onLosePowerDisabled(%data, %obj) {
	if (shouldChangePowerState(%obj,false)) {
		// check to see if a player was using this station
		%occupied = %obj.triggeredBy;
		if(%occupied > 0) {
			if(%data.doesRepair)
				%data.endRepairing(%obj);
			// if it's a deployed station, stop "flashing panels" thread
			if(%data.getName() $= DeployedStationInventory)
				%obj.stopThread($ActivateThread);
			// reset some attributes
			%occupied.setCloaked(false);
			%occupied.station = "";
			%occupied.inStation = false;
			%obj.triggeredBy = "";
			// restore "toggle inventory hud" key
			commandToClient(%occupied.client,'setStationKeys', false);
			// re-mount last weapon or weapon slot 0
			if(%occupied.getMountedImage($WeaponSlot) == 0) {
         // ----------------------------------------------
         // z0dd - ZOD, 5/8/02. Optimized.
         //if(%occupied.inv[%occupied.lastWeapon])
         //   %occupied.use(%occupied.lastWeapon);
         //if(%occupied.getMountedImage($WeaponSlot) == 0) 
         //   %occupied.selectWeaponSlot( 0 );
         if(%occupied.inv[%occupied.lastWeapon])
            %occupied.use(%occupied.lastWeapon);
         else 
            %occupied.selectWeaponSlot( 0 );
         // End z0dd - ZOD
         // ----------------------------------------------
			}
		}
	}
	Parent::onLosePowerDisabled(%data, %obj);
}

// -----------------------------------------------------------------------------
// z0dd - ZOD - Founder(founder@mechina.com), 5/8/02. Totall re-write of Vehicle 
// station creation. More stable, addresses some power related bugs.

// These are not needed anymore.
//function StationVehiclePad::gainPower(%data, %obj)
//{
//   %obj.station.setSelfPowered();
//   Parent::gainPower(%data, %obj);
//}

//function StationVehiclePad::losePower(%data, %obj)
//{
//   %obj.station.clearSelfPowered();
//   Parent::losePower(%data, %obj);
//}
// End z0dd - ZOD - Founder
// -----------------------------------------------------------------------------

//---------------------------------------------------------------------------
// DeployedStationInventory:
//---------------------------------------------------------------------------

function DeployedStationInventory::stationReady(%data, %obj)
{
   %obj.notReady = 1;
   %player = %obj.triggeredBy;
   %obj.playThread($ActivateThread,"activate1");
   // function below found in inventoryHud.cs
   if (!%player.client.isAIControlled())
      buyDeployableFavorites(%player.client);
}

function DeployedStationInventory::stationFinished(%data, %obj)
{
}

function DeployedStationInventory::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      // build offset for player position
      %rot = getWords(%obj.getTransform(), 3, 6);
	%pos = getWords(%colObj.getTransform(),0,2);
	%terrain = getTerrainHeight2(%pos);
	if (!(getWord(%pos,2) < getWord(%terrain,2) || %terrain $= "") || $Host::AllowUnderground)
		//%colObj.setTransform( %pos @ " " @ %rot );
      return true;
   }
   return false;
}

function DeployedStationInventory::onDestroyed(%data, %obj, %prevState)
{
	if (%obj.isRemoved)
		return;
	%obj.isRemoved = true;
   %obj.trigger.delete();
   // decrement team deployed count for this item
   $TeamDeployedCount[%obj.team, InventoryDeployable]--;

   // when a station is destroyed, we don't need its trigger any more
	remDSurface(%obj);
   %obj.schedule(700, "delete");
   Parent::onDestroyed(%data, %obj, %prevState);
}

function StationInventory::onDestroyed(%data,%obj,%prevState) {
	if (%obj.isRemoved)
		return;
	if (%obj.deployed && ($Host::InvincibleDeployables != 1 || %obj.damageFailedDecon)) {
		%obj.isRemoved = true;
		%obj.trigger.delete();
		$TeamDeployedCount[%obj.team,LargeInventoryDeployable]--;
		remDSurface(%obj);
		%obj.schedule(500,"delete");
	}
	Parent::onDestroyed(%data,%obj,%prevState);
}

/// -Deployable Inventory- //////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate
//              animation.
////////////////////////////////////////////////////////////////////////////////
function DeployedStationInventory::getSound(%data, %forward)
{
   if(%forward)
      return "DepInvActivateSound";
   else
      return false;
}

function StationInventory::disassemble(%data,%plyr,%obj) {
	%pl = %obj.triggeredBy;
	%cl = %pl.client;
	if (isObject(%cl)) {
		serverCmdResetControlObject(%cl);
		if (isObject(%pl)) {
			%pl.inStation = "";
			if (!%pl.getMountedImage($WeaponSlot)) {
				if (%pl.inv[%pl.lastWeapon])
					%pl.use(%pl.lastWeapon);
				if (%pl.getMountedImage($WeaponSlot) == 0)
					%pl.selectWeaponSlot(0);
			}
		}
	}
	disassemble(%data,%plyr,%obj);
}

function DeployedStationInventory::disassemble(%data,%plyr,%obj) {
	// Use same code as Large Inventory Station
	StationInventory::disassemble(%data,%plyr,%obj);
}
