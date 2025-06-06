// Notes:
// - respawning vehicles with turrets (bomber/tank) will not setup the turret properly

//Damage Rate for entering Liquid
$VehicleDamageLava       = 0.0325;
$VehicleDamageHotLava    = 0.0325;
$VehicleDamageCrustyLava = 0.0325;

$NumVehiclesDeploy = 0;

//**************************************************************
//* GENERAL PURPOSE FUNCTIONS
//**************************************************************


function serverCmdFoldWings(%client)
{
	if (!isObject(%client.player))
		return;

	%veh = %client.player.getObjectMount();
	if (!isObject(%veh))
	{
		return;
	}

	%veh.setThreadDir($activatethread, false);
	%veh.playThread($activatethread, "activate");
}

function serverCmdUnFoldWings(%client)
{
	if (!isObject(%client.player))
		return;

	%veh = %client.player.getObjectMount();
	if (!isObject(%veh))
	{
		return;
	}

	%veh.setThreadDir($activatethread, true);
	%veh.playThread($activatethread, "activate");
}



function VehicleData::onAdd(%data, %obj) {
	$VehicleList = listAdd($VehicleList,%obj,-1);
   Parent::onAdd(%data, %obj);
   if ((%data.sensorData !$= "") && (%obj.getTarget() != -1))
      setTargetSensorData(%obj.getTarget(), %data.sensorData);
   %obj.setRechargeRate(%data.rechargeRate);
   // set full energy
   %obj.setEnergyLevel(%data.MaxEnergy);

   if (%obj.disableMove)
      %obj.immobilized = true;
   if (%obj.deployed)
   {
      if ($countDownStarted)
         %data.schedule(($Host::WarmupTime * 1000) / 2, "vehicleDeploy", %obj, 0, 1);
      else
      {
         $VehiclesDeploy[$NumVehiclesDeploy] = %obj;
         $NumVehiclesDeploy++;
      }
   }
   if (%obj.mountable || %obj.mountable $= "")
      %data.isMountable(%obj, true);
   else
      %data.isMountable(%obj, false);

   %obj.setSelfPowered();
//   %data.canObserve = true;
}

function VehicleData::onRemove(%this, %obj)
{
   // if there are passengers/driver, kick them out
   %this.deleteAllMounted(%obj);

//[[CHANGE]] Kick commander out as well
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);


   for(%i = 0; %i < %obj.getDatablock().numMountPoints; %i++)
      if (%obj.getMountNodeObject(%i)) {
         %passenger = %obj.getMountNodeObject(%i);
         %passenger.unmount();
      }
   vehicleListRemove(%obj.getDataBlock(), %obj);
   if (%obj.lastPilot.lastVehicle == %obj)
      %obj.lastPilot.lastVehicle = "";

	%loc = findWord($VehicleList,%obj);
	if (%loc !$= "")
		$VehicleList = listDel($VehicleList,%loc);

   //[most] yah.. nukes can now be removed :D
   if (%obj.nuke !$= "")
      {
      %obj.nuke.mpm_all_off(1);
      %obj.unmountObject(%obj.nuke);
      %obj.nuke.schedule(1500,"delete");  
      }
   //[most]

   Parent::onRemove(%this, %obj);
}

function VehicleData::onDamage(%this,%obj)
{
   %damage = %obj.getDamageLevel();
   if (%damage >= %this.destroyedLevel)
   {
      if (%obj.getDamageState() !$= "Destroyed")
      {
         if (%obj.respawnTime !$= "")
            %obj.marker.schedule = %obj.marker.data.schedule(%obj.respawnTime, "respawn", %obj.marker);
         %obj.setDamageState(Destroyed);
      }
   }
   else
   {
      if (%obj.getDamageState() !$= "Enabled")
         %obj.setDamageState(Enabled);
   }
}

function VehicleData::playerDismounted(%data, %obj, %player)
{
   if ( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, true );

   setTargetSensorGroup(%obj.getTarget(), %obj.team);

   // if there is a turret, set its team as well.
   if ( %obj.turretObject > 0 )
      setTargetSensorGroup(%obj.turretObject.getTarget(), %obj.team);
}

function HoverVehicle::useCreateHeight()
{
	//this function is declared to prevent console error msg spam...
}

function WheeledVehicle::useCreateHeight()
{
	//this function is declared to prevent console error msg spam...
}

function AssaultVehicle::onDamage(%this, %obj)
{
   if (isObject(%obj.getMountNodeObject(10)))
      (%obj.getMountNodeObject(10)).setDamagelevel(%obj.getDamageLevel());
   Parent::onDamage(%this, %obj);
}

function BomberFlyer::onDamage(%this, %obj)
{
   if (isObject(%obj.getMountNodeObject(10)))
      (%obj.getMountNodeObject(10)).setDamagelevel(%obj.getDamageLevel());
   Parent::onDamage(%this, %obj);
}

function MobileBaseVehicle::onDamage(%this, %obj)
{
   if (isObject(%obj.getMountNodeObject(1)))
      (%obj.getMountNodeObject(1)).setDamagelevel(%obj.getDamageLevel());
   Parent::onDamage(%this, %obj);
}

function VehicleData::onEnterLiquid(%data, %obj, %coverage, %type)
{
   switch(%type)
   {
      case 0:
         //Water
         %obj.setHeat(0.0);
      case 1:
         //Ocean Water
         %obj.setHeat(0.0);
      case 2:
         //River Water
         %obj.setHeat(0.0);
      case 3:
         //Stagnant Water
         %obj.setHeat(0.0);
      case 4:
         //Lava
         %obj.liquidDamage(%data, $VehicleDamageLava, $DamageType::Lava);
      case 5:
         //Hot Lava
         %obj.liquidDamage(%data, $VehicleDamageHotLava, $DamageType::Lava);
      case 6:
         //Crusty Lava
         %obj.liquidDamage(%data, $VehicleDamageCrustyLava, $DamageType::Lava);
      case 7:
         //Quick Sand
   }
}

function FlyingVehicle::liquidDamage(%obj, %data, %damageAmount, %damageType)
{
   if (%obj.getDamageState() !$= "Destroyed")
   {
      %data.damageObject(%obj, 0, "0 0 0", %damageAmount, %damageType);
      %obj.lDamageSchedule = %obj.schedule(50, "liquidDamage", %data, %damageAmount, %damageType);
      passengerLiquidDamage(%obj, %damageAmount, %damageType);
   }
   else
      %obj.lDamageSchedule = "";
}

function WheeledVehicle::liquidDamage(%obj, %data, %damageAmount, %damageType)
{
   if (%obj.getDamageState() !$= "Destroyed")
   {
      %data.damageObject(%obj, 0, "0 0 0", %damageAmount, %damageType);
      %obj.lDamageSchedule = %obj.schedule(50, "liquidDamage", %data, %damageAmount, %damageType);
      passengerLiquidDamage(%obj, %damageAmount, %damageType);
   }
   else
      %obj.lDamageSchedule = "";
}

function HoverVehicle::liquidDamage(%obj, %data, %damageAmount, %damageType)
{
   if (%obj.getDamageState() !$= "Destroyed")
   {
      %data.damageObject(%obj, 0, "0 0 0", %damageAmount, %damageType);
      %obj.lDamageSchedule = %obj.schedule(50, "liquidDamage", %data, %damageAmount, %damageType);
      passengerLiquidDamage(%obj, %damageAmount, %damageType);
   }
   else
      %obj.lDamageSchedule = "";
}

function passengerLiquidDamage(%obj, %damageAmount, %damageType)
{
//   for(%i = %num; %i < %obj.getDataBlock().numMountPoints; %i++)
//      if (%p = %obj.getMountNodeObject(%i))
//         %p.liquidDamage(%p.getDatablock(), $DamageLava, $DamageType::Lava);
}

function VehicleData::onLeaveLiquid(%data, %obj, %type)
{
   switch(%type)
   {
      case 0:
         //Water
         %obj.setHeat(1.0);
      case 1:
         //Ocean Water
         %obj.setHeat(1.0);
      case 2:
         //River Water
         %obj.setHeat(1.0);
      case 3:
         //Stagnant Water
         %obj.setHeat(1.0);
      case 4:
         //Lava
      case 5:
         //Hot Lava
      case 6:
         //Crusty Lava
      case 7:
         //Quick Sand
   }

   if (%obj.lDamageSchedule !$= "")
   {
      cancel(%obj.lDamageSchedule);
      %obj.lDamageSchedule = "";
   }
}

function VehicleData::onDestroyed(%data, %obj, %prevState) {
	// TODO - temporary - remove
	if ($VehicleDestroyedOverride == 1) {
		%obj.setDamageLevel(0);
		%obj.setDamageState(Enabled);
		return;
	}
    if (%obj.lastDamagedBy)
    {
        %destroyer = %obj.lastDamagedBy;
        game.vehicleDestroyed(%obj, %destroyer);
        //error("vehicleDestroyed( "@ %obj @", "@ %destroyer @")");
    }

	radiusVehicleExplosion(%data, %obj);
   if (%obj.turretObject)
      if (%obj.turretObject.getControllingClient())
         %obj.turretObject.getDataBlock().playerDismount(%obj.turretObject);
   for(%i = 0; %i < %obj.getDatablock().numMountPoints; %i++)
   {
      if (%obj.getMountNodeObject(%i)) {
         %flingee = %obj.getMountNodeObject(%i);
         %flingee.getDataBlock().doDismount(%flingee, true);
         %xVel = 250.0 - (getRandom() * 500.0);
         %yVel = 250.0 - (getRandom() * 500.0);
         %zVel = (getRandom() * 100.0) + 50.0;
         %flingVel = %xVel @ " " @ %yVel @ " " @ %zVel;
         %flingee.applyImpulse(%flingee.getTransform(), %flingVel);
         %flingee.damage(0, %obj.getPosition(), 0.4, $DamageType::Crash);
      }
   }
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);

   %data.deleteAllMounted(%obj);

   // ---------------------------------------------------------------------------------
   // z0dd - ZOD - Czar, 6/24/02. Move this vehicle out of the way so nothing collides 
   // with it.
   if (%data.getName() $= "BomberFlyer" || %data.getName() $= "MobileBaseVehicle" || %data.getName() $= "AssaultVehicle")
   {
      %obj.setFrozenState(true);
      %obj.schedule(2000, "delete");
      %data.schedule(500, 'onAvoidCollisions', %obj);
   }
   else
   {
      %obj.setFrozenState(true); 
      %obj.schedule(500, "delete");
   }
   // ---------------------------------------------------------------------------------
}

// ----------------------------------------------------------------------------------
// z0dd - ZOD, 6/13/02. Move this vehicle out of the way so nothing collides with it.
function VehicleData::onAvoidCollisions(%data, %obj)
{

   // Get the current location of the vehicle and send it to the moon!
   %transform = posFromTransform(%obj.getTransform());
   %position = getWord(%transform, 0) SPC getWord(%transform, 1) SPC (getWord(%transform, 2) - 20000);
   %rotation = getWord(%transform, 3) SPC getWord(%transform, 4) SPC getWord(%transform, 5) SPC getWord(%transform, 6);
   %obj.setTransform(%position SPC %rotation);
}
// ----------------------------------------------------------------------------------

function radiusVehicleExplosion(%data, %vehicle)
{
	// this is a modified version of RadiusExplosion() from projectiles.cs
	%position = %vehicle.getPosition();
   InitContainerRadiusSearch(%position, %data.explosionRadius, $TypeMasks::PlayerObjectType      |
                                                 $TypeMasks::VehicleObjectType     |
                                                 $TypeMasks::MoveableObjectType    |
                                                 $TypeMasks::StaticShapeObjectType |
                                                 $TypeMasks::ForceFieldObjectType  |
                                                 $TypeMasks::TurretObjectType      |
                                                 $TypeMasks::ItemObjectType);

   %numTargets = 0;
   while ((%targetObject = containerSearchNext()) != 0)
   {
		if (%targetObject == %vehicle)
			continue;

      %dist = containerSearchCurrRadDamageDist();

      if (%dist > %data.explosionRadius)
         continue;

      if (!%targetObject.isforcefield() && %targetObject.isMounted())
      {
         %mount = %targetObject.getObjectMount();
			if (%mount == %vehicle)
				continue;

         %found = -1;
         for (%i = 0; %i < %mount.getDataBlock().numMountPoints; %i++)
         {
            if (%mount.getMountNodeObject(%i) == %targetObject)
            {
               %found = %i;
               break;
            }
         }

         if (%found != -1)
         {
            if (%mount.getDataBlock().isProtectedMountPoint[%found] && (%mount != %vehicle))
               continue;
         }
      }

      %targets[%numTargets]     = %targetObject;
      %targetDists[%numTargets] = %dist;
      %numTargets++;
   }

   for (%i = 0; %i < %numTargets; %i++)
   {
      %targetObject = %targets[%i];
      %dist = %targetDists[%i];

      %coverage = calcExplosionCoverage(%position, %targetObject,
                                        ($TypeMasks::InteriorObjectType |
                                         $TypeMasks::TerrainObjectType |
                                         $TypeMasks::ForceFieldObjectType));
      if (%coverage == 0)
         continue;

      %amount = (1.0 - (%dist / %data.explosionRadius)) * %coverage * %data.explosionDamage;
      %targetData = %targetObject.getDataBlock();

      %momVec = "0 0 1";

      if (%amount > 0)
         %targetData.damageObject(%targetObject, %sourceObject, %position, %amount, $DamageType::Explosion, %momVec);
   }
}

function VehicleData::deleteAllMounted()
{

}

//**************************************************************
//* VEHICLE CREATION
//**************************************************************
// (NOTE: No entry for Wildcat Grav Cycle is here. -- DG)

//----------------------------
// SHRIKE SCOUT FLIER
//----------------------------

function ScoutFlyer::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);


   %obj.mountImage(ScoutChaingunParam, 0);
   %obj.mountImage(ScoutChaingunImage, 2);
   %obj.mountImage(ScoutChaingunPairImage, 3);
   %obj.nextWeaponFire = 2;
   %obj.schedule(5500, "playThread", $ActivateThread, "activate");
}

//----------------------------
// THUNDERSWORD BOMBER
//----------------------------

function BomberFlyer::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);

   %turret = TurretData::create(BomberTurret);
   MissionCleanup.add(%turret);
   %turret.team = %obj.teamBought;
   %turret.selectedWeapon = 1;
   %turret.setSelfPowered();
   %obj.mountObject(%turret, 10);
   %turret.mountImage(BomberTurretBarrel,2);
   %turret.mountImage(BomberTurretBarrelPair,3);
   %turret.mountImage(BomberBombImage, 4);
   %turret.mountImage(BomberBombPairImage, 5);
   %turret.mountImage(BomberTargetingImage, 6);
   %obj.turretObject = %turret;
   %turret.setCapacitorRechargeRate( %turret.getDataBlock().capacitorRechargeRate );
   %turret.vehicleMounted = %obj;

   //vehicle turrets should not auto fire at targets
   %turret.setAutoFire(false);

   //for this particular weapon - a non-firing datablock used only so the AI can aim the turret
   //Also needed so we can set the turret parameters..
   %turret.mountImage(AIAimingTurretBarrel, 0);

   // setup the turret's target info
   setTargetSensorGroup(%turret.getTarget(), %turret.team);
   setTargetNeverVisMask(%turret.getTarget(), 0xffffffff);
}

//----------------------------
// HAVOC TRANSPORT FLIER
//----------------------------

function HAPCFlyer::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);
   %obj.schedule(6000, "playThread", $ActivateThread, "activate");
}

//----------------------------
// SUPER HAVOC TRANSPORT FLIER
//----------------------------

function SuperHAPCFlyer::onAdd(%this, %obj) {
	Parent::onAdd(%this, %obj);
	%obj.schedule(6000, "playThread", $ActivateThread, "activate");
}

//----------------------------
// BEOWULF ASSAULT VEHICLE
//----------------------------

function AssaultVehicle::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);

   %turret = TurretData::create(AssaultPlasmaTurret);
   %turret.selectedWeapon = 1;
   MissionCleanup.add(%turret);
   %turret.team = %obj.teamBought;
   %turret.setSelfPowered();
   %obj.mountObject(%turret, 10);
   %turret.mountImage(AssaultPlasmaTurretBarrel, 2);
   %turret.mountImage(AssaultMortarTurretBarrel, 4);
   %turret.setCapacitorRechargeRate( %turret.getDataBlock().capacitorRechargeRate );
   %obj.turretObject = %turret;

   //vehicle turrets should not auto fire at targets
   %turret.setAutoFire(false);

   //Needed so we can set the turret parameters..
   %turret.mountImage(AssaultTurretParam, 0);
   %obj.schedule(6000, "playThread", $ActivateThread, "activate");

   // set the turret's target info
   setTargetSensorGroup(%turret.getTarget(), %turret.team);
   setTargetNeverVisMask(%turret.getTarget(), 0xffffffff);
}

//----------------------------
// JERICHO FORWARD BASE (Mobile Point Base)
//----------------------------

function MobileBaseVehicle::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);
   %obj.station = "";
   %obj.turret = "";
   %obj.beacon = "";

   %obj.schedule(5000, "playThread", $AmbientThread, "ambient");
}

function KillerMobileBaseVehicle::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);
   %obj.station = "";
   %obj.turret = "";
   %obj.beacon = "";

   %obj.schedule(5000, "playThread", $AmbientThread, "ambient");
}
//**************************************************************
//* MULTI-CREW VEHICLE DELETION
//**************************************************************

//----------------------------
//BEOWULF ASSAULT VEHICLE
//----------------------------

function AssaultVehicle::deleteAllMounted(%data, %obj)
{
   %turret = %obj.getMountNodeObject(10);
   if (!%turret)
      return;

   if (%client = %turret.getControllingClient())
   {
      %client.player.setControlObject(%client.player);
      %client.player.mountImage(%client.player.lastWeapon, $WeaponSlot);
      %client.player.mountVehicle = false;
   }
   %turret.schedule(2000, delete);
}

//----------------------------
// THUNDERSWORD BOMBER
//----------------------------

function BomberFlyer::deleteAllMounted(%data, %obj)
{
   if (isObject(%obj.beacon))
      %obj.beacon.schedule(50, delete);

   %turret = %obj.getMountNodeObject(10);
   if (!%turret)
      return;

   %turret.altTrigger = 0;
   %turret.fireTrigger = 0;

   if (%client = %turret.getControllingClient())
   {
      commandToClient(%client, 'endBomberSight');
      %client.player.setControlObject(%client.player);
      %client.player.mountImage(%client.player.lastWeapon, $WeaponSlot);
      %client.player.mountVehicle = false;

      %client.player.bomber = false;
      %client.player.isBomber = false;
   }
   %turret.schedule(2000, delete);
}

//----------------------------
// JERICHO FORWARD BASE
//----------------------------

function MobileBaseVehicle::deleteAllMounted(%data, %obj)
{
   if (%obj.station !$= "")
   {
      %obj.station.getDataBlock().onLosePowerDisabled(%obj.station);
      %obj.unmountObject(%obj.station);
      %obj.station.trigger.schedule(2000, delete);
      %obj.station.schedule(2000, delete);
   }
   if (%obj.turret !$= "")
   {
      %obj.turret.getDataBlock().onLosePowerDisabled(%obj.turret);
      %obj.unmountObject(%obj.turret);
      %obj.turret.schedule(2000, delete);
   }
   //[most]
   if (%obj.nuke !$= "")
      {
      %obj.nuke.mpm_all_off(1);
      %obj.unmountObject(%obj.nuke);
      %obj.nuke.schedule(1500,"delete");  
      }
   //[most]
   if (isObject(%obj.shield))
      %obj.shield.schedule(2000, delete);

   if (isObject(%obj.beacon))
   {
      %obj.beacon.schedule(0, delete);   	
   }
}
function KillerMobileBaseVehicle::deleteAllMounted(%data, %obj)
{
   if (%obj.station !$= "")
   {
      %obj.station.getDataBlock().onLosePowerDisabled(%obj.station);
      %obj.unmountObject(%obj.station);
      %obj.station.trigger.schedule(2000, delete);
      %obj.station.schedule(2000, delete);
   }
   if (%obj.turret !$= "")
   {
      %obj.turret.getDataBlock().onLosePowerDisabled(%obj.turret);
      %obj.unmountObject(%obj.turret);
      %obj.turret.schedule(2000, delete);
   }
   //[most]
   if (%obj.nuke !$= "")
      {
      %obj.nuke.mpm_all_off(1);
      %obj.unmountObject(%obj.nuke);
      %obj.nuke.schedule(1500,"delete");  
      }
   //[most]
   if (isObject(%obj.shield))
      %obj.shield.schedule(2000, delete);

   if (isObject(%obj.beacon))
   {
      %obj.beacon.schedule(0, delete);   	
   }
}
//**************************************************************
//* WEAPON MOUNTING ON VEHICLES
//**************************************************************

//----------------------------
// SHRIKE SCOUT FLIER
//----------------------------

function ScoutFlyer::playerMounted(%data, %obj, %player, %node)
{
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);

   // scout flyer == SUV (single-user vehicle)
   commandToClient(%player.client, 'setHudMode', 'Pilot', "Shrike", %node);
   $numVWeapons = 1;

   // update observers who are following this guy...
   if ( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, false );
}

//----------------------------
// THUNDERSWORD BOMBER
//----------------------------

function BomberFlyer::playerMounted(%data, %obj, %player, %node)
{
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);

   if (%node == 0)
   {
      // pilot position
      %player.setPilot(true);
	   commandToClient(%player.client, 'setHudMode', 'Pilot', "Bomber", %node);
   }
   else if (%node == 1)
   {
      // bombardier position
      %turret = %obj.getMountNodeObject(10);
      %player.vehicleTurret = %turret;
      %player.setTransform("0 0 0 0 0 1 0");
      %player.lastWeapon = %player.getMountedImage($WeaponSlot);
      %player.unmountImage($WeaponSlot);
      if (!%player.client.isAIControlled())
      {
         %player.setControlObject(%turret);
         %player.client.setObjectActiveImage(%turret, 2);
      }
      commandToClient(%player.client, 'startBomberSight');
      %turret.bomber = %player;
      $bWeaponActive = 0;
      %obj.getMountNodeObject(10).selectedWeapon = 1;
      commandToClient(%player.client,'SetWeaponryVehicleKeys', true);

	   commandToClient(%player.client, 'setHudMode', 'Pilot', "Bomber", %node);
      %player.isBomber = true;
   }
   else
   {
      // tail gunner position
	   commandToClient(%player.client, 'setHudMode', 'Passenger', "Bomber", %node);
   }
   // build a space-separated string representing passengers
   // 0 = no passenger; 1 = passenger (e.g. "1 0 0 ")
   %passString = buildPassengerString(%obj);
	// send the string of passengers to all mounted players
	for(%i = 0; %i < %data.numMountPoints; %i++)
		if (%obj.getMountNodeObject(%i) > 0)
		   commandToClient(%obj.getMountNodeObject(%i).client, 'checkPassengers', %passString);

   // update observers who are following this guy...
   if ( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, false );
}

//----------------------------
// HAVOC TRANSPORT FLIER
//----------------------------

function HAPCFlyer::playerMounted(%data, %obj, %player, %node)
{
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);

   if (%node == 0) {
      // pilot position
	   commandToClient(%player.client, 'setHudMode', 'Pilot', "HAPC", %node);
   }
   else {
      // all others
	   commandToClient(%player.client, 'setHudMode', 'Passenger', "HAPC", %node);
   }
   // build a space-separated string representing passengers
   // 0 = no passenger; 1 = passenger (e.g. "1 0 0 1 1 0 ")
   %passString = buildPassengerString(%obj);
	// send the string of passengers to all mounted players
	for(%i = 0; %i < %data.numMountPoints; %i++)
		if (%obj.getMountNodeObject(%i) > 0)
		   commandToClient(%obj.getMountNodeObject(%i).client, 'checkPassengers', %passString);

   // update observers who are following this guy...
   if ( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, false );
}

//----------------------------
// SUPER HAVOC TRANSPORT FLIER
//----------------------------

function SuperHAPCFlyer::playerMounted(%data, %obj, %player, %node) {
	// [[CHANGE]]
	if (%obj.clientControl)
		serverCmdResetControlObject(%obj.clientControl);

	if (%node == 0) {
		// pilot position
		commandToClient(%player.client, 'setHudMode', 'Pilot', "HAPC", %node);
	}
	else {
		// all others
		commandToClient(%player.client, 'setHudMode', 'Passenger', "HAPC", %node);
	}
	// build a space-separated string representing passengers
	// 0 = no passenger; 1 = passenger (e.g. "1 0 0 1 1 0 ")
	%passString = buildPassengerString(%obj);
	// send the string of passengers to all mounted players
	for(%i = 0; %i < %data.numMountPoints; %i++)
		if (%obj.getMountNodeObject(%i) > 0)
			commandToClient(%obj.getMountNodeObject(%i).client, 'checkPassengers', %passString);

	// update observers who are following this guy...
	if ( %player.client.observeCount > 0 )
		resetObserveFollow( %player.client, false );
}

//----------------------------
// WILDCAT GRAV CYCLE
//----------------------------

function ScoutVehicle::playerMounted(%data, %obj, %player, %node)
{
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);

   // scout vehicle == SUV (single-user vehicle)
   commandToClient(%player.client, 'setHudMode', 'Pilot', "Hoverbike", %node);

   // update observers who are following this guy...
   if ( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, false );
}

//----------------------------
// SUPER WILDCAT GRAV CYCLE
//----------------------------

function SuperScoutVehicle::playerMounted(%data, %obj, %player, %node) {
	// [[CHANGE]]
	if (%obj.clientControl)
		serverCmdResetControlObject(%obj.clientControl);

	// scout vehicle == SUV (single-user vehicle)
		commandToClient(%player.client, 'setHudMode', 'Pilot', "Hoverbike", %node);

	// update observers who are following this guy...
	if ( %player.client.observeCount > 0 )
		resetObserveFollow( %player.client, false );
}

//----------------------------
// BEOWULF ASSAULT VEHICLE
//----------------------------

function AssaultVehicle::playerMounted(%data, %obj, %player, %node)
{
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);

   if (%node == 0) {
      // driver position
      // is there someone manning the turret?
      //%turreteer = %obj.getMountedNodeObject(1);
	   commandToClient(%player.client, 'setHudMode', 'Pilot', "Assault", %node);
   }
   else if (%node == 1)
   {
      // turreteer position
      %turret = %obj.getMountNodeObject(10);
      %player.vehicleTurret = %turret;
      %player.setTransform("0 0 0 0 0 1 0");
      %player.lastWeapon = %player.getMountedImage($WeaponSlot);
      %player.unmountImage($WeaponSlot);
      if (!%player.client.isAIControlled())
      {
         %player.setControlObject(%turret);
         %player.client.setObjectActiveImage(%turret, 2);
      }
      %turret.turreteer = %player;
      // if the player is the turreteer, show vehicle's weapon icons
      //commandToClient(%player.client, 'showVehicleWeapons', %data.getName());
      //%player.client.setVWeaponsHudActive(1); // plasma turret icon (default)

      $aWeaponActive = 0;
      commandToClient(%player.client,'SetWeaponryVehicleKeys', true);
      %obj.getMountNodeObject(10).selectedWeapon = 1;
	   commandToClient(%player.client, 'setHudMode', 'Pilot', "Assault", %node);
   }

   // update observers who are following this guy...
   if ( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, false );

   // build a space-separated string representing passengers
   // 0 = no passenger; 1 = passenger (e.g. "1 0 ")
   %passString = buildPassengerString(%obj);
	// send the string of passengers to all mounted players
	for(%i = 0; %i < %data.numMountPoints; %i++)
		if (%obj.getMountNodeObject(%i) > 0)
		   commandToClient(%obj.getMountNodeObject(%i).client, 'checkPassengers', %passString);
}

//----------------------------
// JERICHO FORWARD BASE
//----------------------------

function MobileBaseVehicle::playerMounted(%data, %obj, %player, %node)
{
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);

   // MPB vehicle == SUV (single-user vehicle)
   commandToClient(%player.client, 'setHudMode', 'Pilot', "MPB", %node);
   if (%obj.deploySchedule)
   {
      %obj.deploySchedule.clear();
      %obj.deploySchedule = "";
   }

   if (%obj.deployed !$= "" && %obj.deployed == 1)
   {
      %obj.setThreadDir($DeployThread, false);
      %obj.playThread($DeployThread,"deploy");
      %obj.playAudio($DeploySound, MobileBaseUndeploySound);
      %obj.station.setThreadDir($DeployThread, false);
      %obj.station.getDataBlock().onLosePowerDisabled(%obj.station);
      %obj.station.clearSelfPowered();
      %obj.station.goingOut=false;
      %obj.station.notDeployed = 1;
      %obj.station.playAudio($DeploySound, MobileBaseStationUndeploySound);
      if (isObject(%obj.turret))  
      {
      if ((%turretClient = %obj.turret.getControllingClient()) !$= "")
      {
         CommandToServer( 'resetControlObject', %turretClient );
      }

      %obj.turret.setThreadDir($DeployThread, false);
      %obj.turret.clearTarget();
      %obj.turret.setTargetObject(-1);

      %obj.turret.playAudio($DeploySound, MobileBaseTurretUndeploySound);
      }
       //[most]
      if (isObject(%obj.nuke))
	      %obj.nuke.mpm_all_off(0);
       //[most]     
      %obj.shield.open();
      %obj.shield.schedule(1000,"delete");
      %obj.deploySchedule = "";

      %obj.fullyDeployed = 0;

      %obj.noEnemyControl = 0;
   }
   %obj.deployed = 0;

   // update observers who are following this guy...
   if ( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, false );
}

function KillerMobileBaseVehicle::playerMounted(%data, %obj, %player, %node)
{
//[[CHANGE]]
   if (%obj.clientControl)
       serverCmdResetControlObject(%obj.clientControl);

   // MPB vehicle == SUV (single-user vehicle)
   commandToClient(%player.client, 'setHudMode', 'Pilot', "MPB", %node);
   if (%obj.deploySchedule)
   {
      %obj.deploySchedule.clear();
      %obj.deploySchedule = "";
   }

   if (%obj.deployed !$= "" && %obj.deployed == 1)
   {
      %obj.setThreadDir($DeployThread, false);
      %obj.playThread($DeployThread,"deploy");
      %obj.playAudio($DeploySound, MobileBaseUndeploySound);
      %obj.station.setThreadDir($DeployThread, false);
      %obj.station.getDataBlock().onLosePowerDisabled(%obj.station);
      %obj.station.clearSelfPowered();
      %obj.station.goingOut=false;
      %obj.station.notDeployed = 1;
      %obj.station.playAudio($DeploySound, MobileBaseStationUndeploySound);
      if (isObject(%obj.turret))  
      {
      if ((%turretClient = %obj.turret.getControllingClient()) !$= "")
      {
         CommandToServer( 'resetControlObject', %turretClient );
      }

      %obj.turret.setThreadDir($DeployThread, false);
      %obj.turret.clearTarget();
      %obj.turret.setTargetObject(-1);

      %obj.turret.playAudio($DeploySound, MobileBaseTurretUndeploySound);
      }
       //[most]
      if (isObject(%obj.nuke))
	      %obj.nuke.mpm_all_off(0);
       //[most]     
      %obj.shield.open();
      %obj.shield.schedule(1000,"delete");
      %obj.deploySchedule = "";

      %obj.fullyDeployed = 0;

      %obj.noEnemyControl = 0;
   }
   %obj.deployed = 0;

   // update observers who are following this guy...
   if ( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, false );
}


function buildPassengerString(%vehicle)
{
   %passStr = "";
   for(%i = 0; %i < %vehicle.getDatablock().numMountPoints; %i++)
   {
      if (%vehicle.getMountNodeObject(%i) > 0)
         %passStr = %passStr @ "1 ";
      else
         %passStr = %passStr @ "0 ";
   }

   return %passStr;
}

function MobileBaseVehicle::playerDismounted(%data, %obj, %player)
{
   %obj.schedule(500, "deployVehicle", %data, %player);
   Parent::playerDismounted( %data, %obj, %player );
}

function KillerMobileBaseVehicle::playerDismounted(%data, %obj, %player)
{
   %obj.schedule(500, "deployVehicle", %data, %player);
   Parent::playerDismounted( %data, %obj, %player );
}

function WheeledVehicle::deployVehicle(%obj, %data, %player)
{
   if (!%data.vehicleDeploy(%obj, %player))
      %obj.schedule(500, "deployVehicle", %data, %player);
}

//**************************************************************
//* JERICHO DEPLOYMENT and UNDEPLOYMENT
//**************************************************************

function MobileBaseVehicle::vehicleDeploy(%data, %obj, %player, %force)
{

   if (VectorLen(%obj.getVelocity()) <= 0.1 || %force)
   {
      %deployMessage = "";
      if ( (%deployMessage = %data.checkTurretDistance(%obj)) $= "" || %force)
      {
         if (%obj.station $= "")
         {
            if ( (%deployMessage = %data.checkDeploy(%obj)) $= "" || %force)
            {
               %obj.station = new StaticShape() {
                  scale = "1 1 1";
                  dataBlock = "MobileInvStation";
                  lockCount = "0";
                  homingCount = "0";
                  team = %obj.team;
                  vehicle = %obj;
               };
               %obj.station.startFade(0,0,true);
               %obj.mountObject(%obj.station, 2);
               %obj.station.getDataBlock().createTrigger(%obj.station);
               %obj.station.setSelfPowered();
               %obj.station.playThread($PowerThread,"Power");
               %obj.station.playAudio($HumSound,StationHumSound);
               %obj.station.vehicle = %obj;
             
               //[most] Only give mpb's nukes when enabled in options
               if ($MPM::NukeMPB)
                  {                  
                  %obj.nuke=%obj.Mpm_Turret();
                  %obj.nuke.ammo = %obj.nukeammo;
                  }
               else //Otherwise give normal turret.
                  {
                   %obj.turret = new turret()  
                        {
                  scale = "1 1 1";
                  dataBlock = "MobileTurretBase";
                  lockCount = "0";
                  homingCount = "0";
                  team = %obj.team;
               };
               %obj.turret.setDamageLevel(%obj.getDamageLevel());
               %obj.mountObject(%obj.turret, 1);
               %obj.turret.setSelfPowered();
               %obj.turret.playThread($PowerThread,"Power");
               %obj.turret.mountImage(MissileBarrelLarge, 0 ,false);
                   }
              

               %obj.beacon = new BeaconObject() {
                  dataBlock = "DeployedBeacon";
                  position = %obj.position;
                  rotation = %obj.rotation;
                  team = %obj.team;
               };
               %obj.beacon.setBeaconType(friend);
               %obj.beacon.setTarget(%obj.team);
               // ---------------------------------
               // z0dd - ZOD, 5/8/02. Invalid call.
               //checkSpawnPos(%obj, 20);
            }
         }
         else
         {
            %obj.station.setSelfPowered();
            %obj.station.playThread($PowerThread,"Power");
            //[most] //check if there "is" an turret
            if (isObject(%obj.turret))
               {
            %obj.turret.setSelfPowered();
            %obj.turret.playThread($PowerThread,"Power");
         }
            //[most]
         }
         if (%deployMessage $= "" || %force)
         {
         //[most]
         if (isObject(%obj.turret))
            {
            if (%obj.turret.getTarget() == -1)
            {
               %obj.turret.setTarget(%obj.turret.target);
            }
            %obj.turret.setThreadDir($DeployThread, true);
            %obj.turret.playThread($DeployThread,"deploy");
            %obj.turret.playAudio($DeploySound, MobileBaseTurretDeploySound);
	    }
            if (isObject(%obj.nuke))
	      %obj.nuke.mpm_all_on();
            //[most]
            %obj.station.notDeployed = 1;
            %obj.setThreadDir($DeployThread, true);
            %obj.playThread($DeployThread,"deploy");
            %obj.playAudio($DeploySound, MobileBaseDeploySound);
            %obj.deployed = 1;
            %obj.deploySchedule = "";
            %obj.disableMove = true;
            %obj.setFrozenState(true);
            if (isObject(%obj.shield))
               %obj.shield.delete();

            %obj.shield = new forceFieldBare()
            {
               scale = "1.22 1.8 1.1";
               dataBlock = "defaultTeamSlowFieldBare";
               team = %obj.team;
            };
            %obj.shield.open();
            setTargetSensorData(%obj.getTarget(), MPBDeployedSensor);
         }
      }
      if (%deployMessage !$= "")
         messageClient(%player.client, '', %deployMessage);

      return true;
   }
   else
   {
      return false;
   }
}

function MobileBaseVehicle::onEndSequence(%data, %obj, %thread)
{
   if (%thread == $DeployThread && !%obj.deployed)
   {
      %obj.unmountObject(%obj.station);
      %obj.station.trigger.delete();
      %obj.station.delete();
      %obj.station = "";

	  %obj.beacon.delete();
      //[most] Handles removal of nuke.
      if (isObject(%obj.nuke))
         {
         %obj.nukeammo = %obj.nuke.ammo;
         %obj.nuke.mpm_all_off(1);
         %obj.unmountObject(%obj.nuke);
         %obj.nuke.schedule(1500,"delete");      
         }
      if (isObject(%obj.turret))
         {
      %obj.unmountObject(%obj.turret);
      %obj.turret.delete();
      %obj.turret = "";
         }
      //[most]
      if (!%obj.immobilized)
      {
         %obj.disableMove = false;
         %obj.setFrozenState(false);
      }
      setTargetSensorData(%obj.getTarget(), %data.sensorData);
   }
   else
   {
      %obj.station.startFade(0,0,false);
      %obj.station.setThreadDir($DeployThread, true);
      %obj.station.playThread($DeployThread,"deploy");
      %obj.station.playAudio($DeploySound, MobileBaseStationDeploySound);
      %obj.station.goingOut = true;
      %obj.shield.setTransform(%obj.getSlotTransform(3));
      %obj.shield.close();
      %obj.isDeployed = true;
      %obj.noEnemyControl = 1;
   }

   Parent::onEndSequence(%data, %obj, %thread);
}

function MobileInvStation::onEndSequence(%data, %obj, %thread)
{
   if (!%obj.goingOut)
      %obj.startFade(0,0,true);
   else
   {
      %obj.notDeployed = 0;
      %obj.vehicle.fullyDeployed = 1;
   }
   Parent::onEndSequence(%data, %obj, %thread);
}


function MobileBaseVehicle::checkDeploy(%data, %obj)
{

   %mask = $TypeMasks::VehicleObjectType | $TypeMasks::MoveableObjectType |
           $TypeMasks::StaticShapeObjectType | $TypeMasks::ForceFieldObjectType |
           $TypeMasks::ItemObjectType | $TypeMasks::PlayerObjectType |
           $TypeMasks::TurretObjectType | //$TypeMasks::StaticTSObjectType |
           $TypeMasks::InteriorObjectType;

   //%slot 1 = turret   %slot 2 = station
   %height[1] = 0;
   %height[2] = 0;
   %radius[1] = 2.4;
   %radius[2] = 2.4;
   %stationFailed = false;
   %turretFailed = false;

   for(%x = 1; %x < 3; %x++)
   {
      %posXY = getWords(%obj.getSlotTransform(%x), 0, 1);
      %posZ = (getWord(%obj.getSlotTransform(%x), 2) + %height[%x]);
      //InitContainerRadiusSearch(%posXY @ " " @ %posZ, %radius[%x], %mask);

      while ((%objFound = ContainerSearchNext()) != 0)
      {
         if (%objFound != %obj)
         {
            if (%x == 1)
               %turretFailed = true;
            else
               %stationFailed = true;
            break;
         }
      }
   }

   //If turret, station or both fail the send back the error message...
   if (%turretFailed &&  %stationFailed)
      return "Both Turret and Station are blocked and unable to deploy.";
   if (%turretFailed)
      return "Turret is blocked and unable to deploy.";
   if (%stationFailed)
      return "Station is blocked and unable to deploy.";

   //Check the station for collision with the Terrain
   %mat = %obj.getTransform();
   for(%x = 1; %x < 7; %x+=2)
   {
      %startPos = MatrixMulPoint(%mat, %data.stationPoints[%x]);
      %endPos = MatrixMulPoint(%mat, %data.stationPoints[%x+1]);

      //%rayCastObj = containerRayCast(%startPos, %endPos, $TypeMasks::TerrainObjectType, 0);
      if (%rayCastObj)
         return "Station is blocked by terrain and unable to deploy.";
   }

   return "";
}
function MobileBaseVehicle::checkTurretDistance(%data, %obj)
{
   %pos = getWords(%obj.getTransform(), 0, 2);
   //InitContainerRadiusSearch(%pos, 100, $TypeMasks::TurretObjectType | $TypeMasks::InteriorObjectType);
   while ((%objFound = ContainerSearchNext()) != 0)
   {
      if (%objFound.getType() & $TypeMasks::TurretObjectType)
      {
         if (%objFound.getDataBlock().ClassName $= "TurretBase")
            return "Turret Base is in the area. Unable to deploy.";
      }
      else
      {
         %subStr = getSubStr(%objFound.interiorFile, 1, 4);
         if (%subStr !$= "rock" && %subStr !$= "spir" && %subStr !$= "misc")
            return "Building is in the area. Unable to deploy.";
      }
   }
   return "";
}

function KillerMobileBaseVehicle::vehicleDeploy(%data, %obj, %player, %force)
{

   if (VectorLen(%obj.getVelocity()) <= 0.1 || %force)
   {
      %deployMessage = "";
      if ( (%deployMessage = %data.checkTurretDistance(%obj)) $= "" || %force)
      {
         if (%obj.station $= "")
         {
            if ( (%deployMessage = %data.checkDeploy(%obj)) $= "" || %force)
            {
               %obj.station = new StaticShape() {
                  scale = "1 1 1";
                  dataBlock = "MobileInvStation";
                  lockCount = "0";
                  homingCount = "0";
                  team = %obj.team;
                  vehicle = %obj;
               };
               %obj.station.startFade(0,0,true);
               %obj.mountObject(%obj.station, 2);
               %obj.station.getDataBlock().createTrigger(%obj.station);
               %obj.station.setSelfPowered();
               %obj.station.playThread($PowerThread,"Power");
               %obj.station.playAudio($HumSound,StationHumSound);
               %obj.station.vehicle = %obj;
             
               //[most] Only give mpb's nukes when enabled in options
               if ($MPM::NukeMPB)
                  {                  
                  %obj.nuke=%obj.Mpm_Turret();
                  %obj.nuke.ammo = %obj.nukeammo;
                  }
               else //Otherwise give normal turret.
                  {
                   %obj.turret = new turret()  
                        {
                  scale = "1 1 1";
                  dataBlock = "MobileTurretBase";
                  lockCount = "0";
                  homingCount = "0";
                  team = %obj.team;
               };
               %obj.turret.setDamageLevel(%obj.getDamageLevel());
               %obj.mountObject(%obj.turret, 1);
               %obj.turret.setSelfPowered();
               %obj.turret.playThread($PowerThread,"Power");
               %obj.turret.mountImage(MissileBarrelLarge, 0 ,false);
                   }
              

               %obj.beacon = new BeaconObject() {
                  dataBlock = "DeployedBeacon";
                  position = %obj.position;
                  rotation = %obj.rotation;
                  team = %obj.team;
               };
               %obj.beacon.setBeaconType(friend);
               %obj.beacon.setTarget(%obj.team);
               // ---------------------------------
               // z0dd - ZOD, 5/8/02. Invalid call.
               //checkSpawnPos(%obj, 20);
            }
         }
         else
         {
            %obj.station.setSelfPowered();
            %obj.station.playThread($PowerThread,"Power");
            //[most] //check if there "is" an turret
            if (isObject(%obj.turret))
               {
            %obj.turret.setSelfPowered();
            %obj.turret.playThread($PowerThread,"Power");
         }
            //[most]
         }
         if (%deployMessage $= "" || %force)
         {
         //[most]
         if (isObject(%obj.turret))
            {
            if (%obj.turret.getTarget() == -1)
            {
               %obj.turret.setTarget(%obj.turret.target);
            }
            %obj.turret.setThreadDir($DeployThread, true);
            %obj.turret.playThread($DeployThread,"deploy");
            %obj.turret.playAudio($DeploySound, MobileBaseTurretDeploySound);
	    }
            if (isObject(%obj.nuke))
	      %obj.nuke.mpm_all_on();
            //[most]
            %obj.station.notDeployed = 1;
            %obj.setThreadDir($DeployThread, true);
            %obj.playThread($DeployThread,"deploy");
            %obj.playAudio($DeploySound, MobileBaseDeploySound);
            %obj.deployed = 1;
            %obj.deploySchedule = "";
            %obj.disableMove = true;
            %obj.setFrozenState(true);
            if (isObject(%obj.shield))
               %obj.shield.delete();

            %obj.shield = new forceFieldBare()
            {
               scale = "1.22 1.8 1.1";
               dataBlock = "defaultTeamSlowFieldBare";
               team = %obj.team;
            };
            %obj.shield.open();
            setTargetSensorData(%obj.getTarget(), MPBDeployedSensor);
         }
      }
      if (%deployMessage !$= "")
         messageClient(%player.client, '', %deployMessage);

      return true;
   }
   else
   {
      return false;
   }
}

function KillerMobileBaseVehicle::onEndSequence(%data, %obj, %thread)
{
   if (%thread == $DeployThread && !%obj.deployed)
   {
      %obj.unmountObject(%obj.station);
      %obj.station.trigger.delete();
      %obj.station.delete();
      %obj.station = "";

	  %obj.beacon.delete();
      //[most] Handles removal of nuke.
      if (isObject(%obj.nuke))
         {
         %obj.nukeammo = %obj.nuke.ammo;
         %obj.nuke.mpm_all_off(1);
         %obj.unmountObject(%obj.nuke);
         %obj.nuke.schedule(1500,"delete");      
         }
      if (isObject(%obj.turret))
         {
      %obj.unmountObject(%obj.turret);
      %obj.turret.delete();
      %obj.turret = "";
         }
      //[most]
      if (!%obj.immobilized)
      {
         %obj.disableMove = false;
         %obj.setFrozenState(false);
      }
      setTargetSensorData(%obj.getTarget(), %data.sensorData);
   }
   else
   {
      %obj.station.startFade(0,0,false);
      %obj.station.setThreadDir($DeployThread, true);
      %obj.station.playThread($DeployThread,"deploy");
      %obj.station.playAudio($DeploySound, MobileBaseStationDeploySound);
      %obj.station.goingOut = true;
      %obj.shield.setTransform(%obj.getSlotTransform(3));
      %obj.shield.close();
      %obj.isDeployed = true;
      %obj.noEnemyControl = 1;
   }

   Parent::onEndSequence(%data, %obj, %thread);
}


function KillerMobileBaseVehicle::checkDeploy(%data, %obj)
{

   %mask = $TypeMasks::VehicleObjectType | $TypeMasks::MoveableObjectType |
           $TypeMasks::StaticShapeObjectType | $TypeMasks::ForceFieldObjectType |
           $TypeMasks::ItemObjectType | $TypeMasks::PlayerObjectType |
           $TypeMasks::TurretObjectType | //$TypeMasks::StaticTSObjectType |
           $TypeMasks::InteriorObjectType;

   //%slot 1 = turret   %slot 2 = station
   %height[1] = 0;
   %height[2] = 0;
   %radius[1] = 2.4;
   %radius[2] = 2.4;
   %stationFailed = false;
   %turretFailed = false;

   for(%x = 1; %x < 3; %x++)
   {
      %posXY = getWords(%obj.getSlotTransform(%x), 0, 1);
      %posZ = (getWord(%obj.getSlotTransform(%x), 2) + %height[%x]);
      //InitContainerRadiusSearch(%posXY @ " " @ %posZ, %radius[%x], %mask);

      while ((%objFound = ContainerSearchNext()) != 0)
      {
         if (%objFound != %obj)
         {
            if (%x == 1)
               %turretFailed = true;
            else
               %stationFailed = true;
            break;
         }
      }
   }

   //If turret, station or both fail the send back the error message...
   if (%turretFailed &&  %stationFailed)
      return "Both Turret and Station are blocked and unable to deploy.";
   if (%turretFailed)
      return "Turret is blocked and unable to deploy.";
   if (%stationFailed)
      return "Station is blocked and unable to deploy.";

   //Check the station for collision with the Terrain
   %mat = %obj.getTransform();
   for(%x = 1; %x < 7; %x+=2)
   {
      %startPos = MatrixMulPoint(%mat, %data.stationPoints[%x]);
      %endPos = MatrixMulPoint(%mat, %data.stationPoints[%x+1]);

      //%rayCastObj = containerRayCast(%startPos, %endPos, $TypeMasks::TerrainObjectType, 0);
      if (%rayCastObj)
         return "Station is blocked by terrain and unable to deploy.";
   }

   return "";
}
function KillerMobileBaseVehicle::checkTurretDistance(%data, %obj)
{
   %pos = getWords(%obj.getTransform(), 0, 2);
   //InitContainerRadiusSearch(%pos, 100, $TypeMasks::TurretObjectType | $TypeMasks::InteriorObjectType);
   while ((%objFound = ContainerSearchNext()) != 0)
   {
      if (%objFound.getType() & $TypeMasks::TurretObjectType)
      {
         if (%objFound.getDataBlock().ClassName $= "TurretBase")
            return "Turret Base is in the area. Unable to deploy.";
      }
      else
      {
         %subStr = getSubStr(%objFound.interiorFile, 1, 4);
         if (%subStr !$= "rock" && %subStr !$= "spir" && %subStr !$= "misc")
            return "Building is in the area. Unable to deploy.";
      }
   }
   return "";
}


//**************************************************************
//* VEHICLE INVENTORY MANAGEMENT
//**************************************************************

//--------------------------------------------------------------
// NUMBER OF PURCHASEABLE VEHICLES PER TEAM
//--------------------------------------------------------------

$VehicleRespawnTime            = 15000;
$Vehiclemax[ScoutVehicle]      = 4;
$Vehiclemax[SuperScoutVehicle] = 4;
$VehicleMax[AssaultVehicle]    = 3;
$VehicleMax[MobileBaseVehicle] = 1;
$VehicleMax[ScoutFlyer]        = 4;
$VehicleMax[BomberFlyer]       = 2;
$VehicleMax[HAPCFlyer]         = 2;
$VehicleMax[SuperHAPCFlyer]    = 2;
$VehicleMax[Artillery]         = 0;


function vehicleListRemove(%data, %obj)
{
   %blockName = %data.getName();
   for($i = 0; %i < $VehicleMax[%blockName]; %i++)
      if ($VehicleInField[%obj.team, %blockName, %i] == %obj)
      {
         $VehicleInField[%obj.team, %blockName, %i] = 0;
         $VehicleTotalCount[%obj.team, %blockName]--;
         break;
      }
}

function vehicleListAdd(%blockName, %obj)
{
   for($i = 0; %i < $VehicleMax[%blockName]; %i++)
   {
      if ($VehicleInField[%obj.team, %blockName, %i] $= "" || $VehicleInField[%obj.team, %blockName, %i] == 0)
      {
         $VehicleInField[%obj.team, %blockName, %i] = %obj;
         $VehicleTotalCount[%obj.team, %blockName]++;
         break;
      }
   }
}

function clearVehicleCount(%team)
{
   $VehicleTotalCount[%team, ScoutVehicle]      = 0;
   $VehicleTotalCount[%team, SuperScoutVehicle] = 0;
   $VehicleTotalCount[%team, AssaultVehicle]    = 0;
   $VehicleTotalCount[%team, MobileBaseVehicle] = 0;
   $VehicleTotalCount[%team, ScoutFlyer]        = 0;
   $VehicleTotalCount[%team, BomberFlyer]       = 0;
   $VehicleTotalCount[%team, HAPCFlyer]         = 0;
   $VehicleTotalCount[%team, SuperHAPCFlyer]    = 0;
   $VehicleTotalCount[%team, Artillery]         = 0;
}

//**************************************************************
//* VEHICLE HUD SEAT INDICATOR LIGHTS
//**************************************************************

// ---------------------------------------------------------
// z0dd - ZOD, 6/18/02. Get the name of the vehicle node and 
// pass to Armor::onMount and Armor::onDismount in player.cs
function findNodeName(%vehicle, %node)
{

   %vName = %vehicle.getDataBlock().getName();
   if (%vName !$= "HAPCFlyer")
   {
      if (%node == 0)
         return 'pilot';
      else if (%node == 1)
         return 'gunner';
      else
         return 'tailgunner';
   }
   else
   {
      if (%node == 0)
         return 'pilot';
      else if (%node == 1)
         return 'tailgunner';
      else
         return 'passenger';
   }
}
// End z0dd - ZOD
// ---------------------------------------------------------

function findAIEmptySeat(%vehicle, %player)
{
   %dataBlock = %vehicle.getDataBlock();
	if (%dataBlock.getName() $= "BomberFlyer")
		%num = 2;
	else
		%num = 1;
	%node = -1;
	for(%i = %num; %i < %dataBlock.numMountPoints; %i++)
	{
	   if (!%vehicle.getMountNodeObject(%i))
	   {
	      //cheap hack - for now, AI's will mount the next available node regardless of where they collided
	      %node = %i;
	      break;
	   }
	}

	//return the empty seat
	return %node;
}

function findEmptySeat(%vehicle, %player, %forceNode)
{
   %minNode = 1;
   %node = -1;
   %dataBlock = %vehicle.getDataBlock();
   %dis = %dataBlock.minMountDist;
   %playerPos = getWords(%player.getTransform(), 0, 2);
   %message = "";
   if (%dataBlock.lightOnly)
   {
      if (%player.client.armor $= "Light" || %player.client.armor $= "Pure")
         %minNode = 0;
      else
         %message = '\c2Only Scout Armors can pilot this vehicle.~wfx/misc/misc.error.wav';
   }
   else if (%player.client.armor $= "Light" || %player.client.armor $= "Medium" || %player.client.armor $= "Pure")
      %minNode = 0;
   else
      %minNode = findFirstHeavyNode(%dataBlock);

   if (%forceNode !$= "")
      %node = %forceNode;
   else
   {
      for(%i = 0; %i < %dataBlock.numMountPoints; %i++)
         if (!%vehicle.getMountNodeObject(%i))
         {
            %seatPos = getWords(%vehicle.getSlotTransform(%i), 0, 2);
            %disTemp = VectorLen(VectorSub(%seatPos, %playerPos));
            if (%disTemp <= %dis)
            {
               %node = %i;
               %dis = %disTemp;
            }
         }
    }
   if (%node != -1 && %node < %minNode)
   {
      if (%message $= "")
      {
         if (%node == 0)
            %message = '\c2Only Scout, Assault or Pure Armors can pilot this vehicle.~wfx/misc/misc.error.wav';
         else
            %message = '\c2Only Scout, Assault or Pure Armors can use that position.~wfx/misc/misc.error.wav';
      }

      if (!%player.noSitMessage)
      {
         %player.noSitMessage = true;
         %player.schedule(2000, "resetSitMessage");
         messageClient(%player.client, 'MsgArmorCantMountVehicle', %message);
      }
      %node = -1;
   }
   return %node;
}

function findFirstHeavyNode(%data)
{
   for(%i = 0; %i < %data.numMountPoints; %i++)
      if (%data.mountPose[%i] $= "")
         return %i;
   return %data.numMountPoints;
}

//**************************************************************
//* DAMAGE FUNCTIONS
//**************************************************************

function VehicleData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %theClient, %proj)
{

   if (%proj !$= "")
   {
      if (%amount > 0 && %targetObject.lastDamageProj !$= %proj)
      {
         %targetObject.lastDamageProj = %proj;
         %targetObject.lastDamageAmount = %amount;
      }
      else if (%targetObject.lastDamageAmount < %amount)
         %amount = %amount - %targetObject.lastDamageAmount;
      else
         return;
   }

// TODO - check
   // check for team damage
	if (isObject(%sourceObject))
		%sourceClient = %sourceObject ? %sourceObject.getOwnerClient() : 0;
	%targetTeam = getTargetSensorGroup(%targetObject.getTarget());

	if (%sourceClient)
		%sourceTeam = %sourceClient.getSensorGroup();
	else if (isObject(%sourceObject) && %sourceObject.getClassName() $= "Turret")
		%sourceTeam = getTargetSensorGroup(%sourceObject.getTarget());
	else
		%sourceTeam = %sourceObject ? getTargetSensorGroup(%sourceObject.getTarget()) : -1;

    // vehicles no longer obey team damage -JR
//    if (!$teamDamage && (%targetTeam == %sourceTeam) && %targetObject.getDamagePercent() > 0.5)
//       return;
    //but we do want to track the destroyer
    if (%sourceObject)
    {
        %targetObject.lastDamagedBy = %sourceObject;
        %targetObject.lastDamageType = %damageType;
    }
    else
        %targetObject.lastDamagedBy = 0;


   // Scale damage type & include shield calculations...
   if (%data.isShielded)
      %amount = %data.checkShields(%targetObject, %position, %amount, %damageType);


   %damageScale = %data.damageScale[%damageType];
   if (%damageScale !$= "")
      %amount *= %damageScale;

   if (%amount != 0)
      %targetObject.applyDamage(%amount);

   if (%targetObject.getDamageState() $= "Destroyed" )
   {
      if ( %momVec !$= "")
         %targetObject.setMomentumVector(%momVec);
   }
}

function VehicleData::onImpact(%data, %vehicleObject, %collidedObject, %vec, %vecLen)
{
 	
   if (%vecLen > %data.minImpactSpeed)
      %data.damageObject(%vehicleObject, 0, VectorAdd(%vec, %vehicleObject.getPosition()),
                         %vecLen * %data.speedDamageScale, $DamageType::Ground);

   // associated "crash" sounds
   if (%vecLen > %vDataBlock.hardImpactSpeed)
      %vehicleObject.playAudio(0, %vDataBlock.hardImpactSound);
   else if (%vecLen > %vDataBlock.softImpactSpeed)
      %vehicleObject.playAudio(0, %vDataBlock.softImpactSound);
}

//**************************************************************
//* VEHICLE TIMEOUTS
//**************************************************************

function vehicleAbandonTimeOut(%vehicle)
{
   if (%vehicle.getDatablock().cantAbandon $= "" && %vehicle.lastPilot $= "")
   {
      for(%i = 0; %i < %vehicle.getDatablock().numMountPoints; %i++)
         if (%vehicle.getMountNodeObject(%i))
         {
            %passenger = %vehicle.getMountNodeObject(%i);
            if (%passenger.lastVehicle !$= "")
               schedule(2400, %passenger.lastVehicle,"vehicleAbandonTimeOut", %passenger.lastVehicle);
            %passenger.lastVehicle = %vehicle;
            %vehicle.lastPilot = %passenger;
            return;
         }

      if (%vehicle.respawnTime !$= "")
         %vehicle.marker.schedule = %vehicle.marker.data.schedule(%vehicle.respawnTime, "respawn", %vehicle.marker);
      %vehicle.mountable = 0;
      %vehicle.startFade(2400, 0, true);
      %vehicle.schedule(2401, "delete");
   }
}

//------------------------------------------------------------------------------
function HoverVehicleData::create(%block, %team, %oldObj)
{
   if (%oldObj $= "")
   {
      %obj = new HoverVehicle()
      {
         dataBlock  = %block;
         respawn    = "0";
         teamBought = %team;
         team = %team;
      };
   }
   else
   {
      %obj = new HoverVehicle()
      {
         dataBlock  = %data;
         respawn    = "0";
         teamBought = %team;
         team = %team;
         mountable = %oldObj.mountable;
         disableMove = %oldObj.disableMove;
         resetPos = %oldObj.resetPos;
         respawnTime = %oldObj.respawnTime;
         marker = %oldObj;
      };
   }
   return(%obj);
}

function WheeledVehicleData::create(%data, %team, %oldObj)
{
   if (%oldObj $= "")
   {
      %obj = new WheeledVehicle()
      {
         dataBlock  = %data;
         respawn    = "0";
         teamBought = %team;
         team = %team;
      };
   }
   else
   {
      %obj = new WheeledVehicle()
      {
         dataBlock  = %data;
         respawn    = "0";
         teamBought = %team;
         team = %team;
         mountable = %oldObj.mountable;
         disableMove = %oldObj.disableMove;
         resetPos = %oldObj.resetPos;
         deployed = %oldObj.deployed;
         respawnTime = %oldObj.respawnTime;
         marker = %oldObj;
      };
   }

   return(%obj);
}

function FlyingVehicleData::create(%data, %team, %oldObj)
{
   if (%oldObj $= "")
   {
      %obj = new FlyingVehicle()
      {
         dataBlock  = %data;
         respawn    = "0";
         teamBought = %team;
         team = %team;
      };
   }
   else
   {
      %obj = new FlyingVehicle()
      {
         dataBlock  = %data;
         teamBought = %team;
         team = %team;
         mountable = %oldObj.mountable;
         disableMove = %oldObj.disableMove;
         resetPos = %oldObj.resetPos;
         respawnTime = %oldObj.respawnTime;
         marker = %oldObj;
      };
   }

   return(%obj);
}

function FlyingVehicleData::switchSidesSetPos(%data, %oldObj)
{
   %team = %oldObj.curTeam == 1 ? 2 : 1;
   %oldObj.curTeam = %team;
   %obj = new FlyingVehicle()
   {
      dataBlock  = %data;
      teamBought = %team;
      team = %team;
      mountable = %oldObj.mountable;
      disableMove = %oldObj.disableMove;
      resetPos = %oldObj.resetPos;
      respawnTime = %oldObj.respawnTime;
      marker = %oldObj;
   };
   %obj.setTransform(%oldObj.getTransform());

   return(%obj);
}

function WheeledVehicleData::switchSidesSetPos(%data, %oldObj)
{
   %team = %oldObj.curTeam == 1 ? 2 : 1;
   %oldObj.curTeam = %team;
   %obj = new WheeledVehicle()
   {
      dataBlock  = %data;
      respawn    = "0";
      teamBought = %team;
      team = %team;
      mountable = %oldObj.mountable;
      disableMove = %oldObj.disableMove;
      resetPos = %oldObj.resetPos;
      deployed = %oldObj.deployed;
      respawnTime = %oldObj.respawnTime;
      marker = %oldObj;
   };
   %obj.setTransform(%oldObj.getTransform());
   return(%obj);
}

function HoverVehicleData::switchSides(%data, %oldObj)
{
   %team = %oldObj.curTeam == 1 ? 2 : 1;
   %oldObj.curTeam = %team;
   %obj = new HoverVehicle()
   {
      dataBlock  = %data;
      respawn    = "0";
      teamBought = %team;
      team = %team;
      mountable = %oldObj.mountable;
      disableMove = %oldObj.disableMove;
      resetPos = %oldObj.resetPos;
      respawnTime = %oldObj.respawnTime;
      marker = %oldObj;
   };
   %obj.setTransform(%oldObj.getTransform());
   return(%obj);
}

function resetNonStaticObjPositions()
{
   MissionGroup.setupPositionMarkers(false);
   MissionCleanup.positionReset();
}

function next(%team)
{
   ResetObjsPositions(%team);
}

function SimGroup::positionReset(%group)
{
   for(%i = %group.getCount() - 1; %i >=0 ; %i--)
   {
      %obj = %group.getObject(%i);
      if (%obj.resetPos && %obj.getName() !$= PosMarker)
         %obj.delete();
      else
         %obj.positionReset();
   }

   for(%i = 0; %i < %group.getCount(); %i++)
   {
      %obj = %group.getObject(%i);
      if (%obj.getName() $= PosMarker)
      {
         cancel(%obj.schedule);
         %newObj =  %obj.data.switchSidesSetPos(%obj);
         MissionCleanup.add(%newObj);
         setTargetSensorGroup(%newObj.target, %newObj.team);
      }
      else
         %obj.positionReset();
   }
}

function VehicleData::respawn(%data, %marker)
{
   %mask = $TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::TurretObjectType;
   InitContainerRadiusSearch(%marker.getWorldBoxCenter(), %data.checkRadius, %mask);
   if (containerSearchNext() == 0)
   {
      %newObj = %data.create(%marker.curTeam, %marker);
      %newObj.startFade(1000, 0, false);
      %newObj.setTransform(%marker.getTransform());

      setTargetSensorGroup(%newObj.target, %newObj.team);
      MissionCleanup.add(%newObj);
   }
   else
   {
      %marker.schedule = %data.schedule(3000, "respawn", %marker);
   }
}

function SimObject::positionReset(%group, %team)
{
   //Used to avoid warnings
}

function Terraformer::positionReset(%group, %team)
{
   //Used to avoid warnings
}

function SimGroup::setupPositionMarkers(%group, %create)
{
   for(%i = %group.getCount() - 1; %i >= 0; %i--)
   {
      %obj = %group.getObject(%i);
      if (%obj.resetPos || %obj.respawnTime !$= "")
      {
         if (%create)
         {
            %marker = %obj.getDataBlock().createPositionMarker(%obj);
            MissionCleanup.add(%marker);
            %obj.marker = %marker;
         }
         else
         {
            %obj.delete();
         }
      }
      else
         %obj.setupPositionMarkers(%create);
   }
}

function SimObject::setupPositionMarkers(%group, %create)
{
   //Used to avoid warnings
}

function VehicleData::createPositionMarker(%data, %obj)
{
   %marker = new Trigger(PosMarker)
   {
      dataBlock = markerTrigger;
      mountable = %obj.mountable;
      disableMove = %obj.disableMove;
      resetPos = %obj.resetPos;
      data = %obj.getDataBlock().getName();
      deployed = %obj.deployed;
      curTeam = %obj.team;
      respawnTime = %obj.respawnTime;
   };
   %marker.setTransform(%obj.getTransform());
   return %marker;
}

function VehicleData::hasDismountOverrides(%data, %obj)
{
   return false;
}

