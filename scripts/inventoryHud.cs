//------------------------------------------------------------------------------
function setUpFavPrefs()
{
   if($pref::FavCurrentSelect $= "")
      $pref::FavCurrentSelect = 0;
   for(%i = 0; %i < 10; %i++)
   {
      if($pref::FavNames[%i] $= "")
         $pref::FavNames[%i] = "Favorite " @ %i + 1;
      if($pref::Favorite[%i] $= "")
         $pref::Favorite[%i] = "armor\tLight Armor";
   }
   if($pref::FavCurrentList $= "")
      $pref::FavCurrentList = 0;
}

$FavCurrent = 0;
setUpFavPrefs();

$InvArmor[0] = "Scout";
$InvArmor[1] = "Assault";
$InvArmor[2] = "Juggernaut";

$NameToInv["Scout"]  = "Light";
$NameToInv["Assault"] = "Medium";
$NameToInv["Juggernaut"]  = "Heavy";
$NameToInv["Purebuild"]  = "Pure";

$InvWeapon[0] = "Blaster";
$InvWeapon[1] = "Plasma Rifle";
$InvWeapon[2] = "Chaingun";
$InvWeapon[3] = "Spinfusor";
$InvWeapon[4] = "Grenade Launcher";
$InvWeapon[5] = "Laser Rifle";
$InvWeapon[6] = "ELF Projector";
$InvWeapon[7] = "Fusion Mortar";
$InvWeapon[8] = "Missile Launcher";
$InvWeapon[9] = "Shocklance";
//$InvWeapon[10] = "Targeting Laser";
// AO
$InvWeapon[10] = "TR2 Spinfusor";
$InvWeapon[11] = "TR2 Grenade Launcher";
$InvWeapon[12] = "TR2 Chaingun";
$InvWeapon[13] = "TR2 Shocklance";
$InvWeapon[14] = "TR2 Mortar";
// END AO

$InvWeapon[15] = "Construction Tool";
$InvWeapon[16] = "Nerf Gun";
$InvWeapon[17] = "Nerf Ball Launcher";
$InvWeapon[18] = "Super Chaingun";
$InvWeapon[19] = "Transport gun";
$InvWeapon[20] = "Modifier Tool";

$NameToInv["Blaster"] = "Blaster";
$NameToInv["Plasma Rifle"] = "Plasma";
$NameToInv["Chaingun"] = "Chaingun";
$NameToInv["Spinfusor"] = "Disc";
$NameToInv["Grenade Launcher"] = "GrenadeLauncher";
$NameToInv["Laser Rifle"] = "SniperRifle";
$NameToInv["ELF Projector"] = "ELFGun";
$NameToInv["Fusion Mortar"] = "Mortar";
$NameToInv["Missile Launcher"] = "MissileLauncher";
$NameToInv["Shocklance"] = "ShockLance";
$NameToInv["Construction Tool"] = "ConstructionTool";
$NameToInv["Nerf Gun"] = "NerfGun";
$NameToInv["Nerf Ball Launcher"] = "NerfBallLauncher";
$NameToInv["Super Chaingun"] = "SuperChaingun";
$NameToInv["Transport gun"] = "Transgun";
$NameToInv["Modifier Tool"] = "MergeTool";
//$NameToInv["Targeting Laser"] = "TargetingLaser";
// AO
$NameToInv["TR2 Spinfusor"] = "TR2Disc";
$NameToInv["TR2 Grenade Launcher"] = "TR2GrenadeLauncher";
$NameToInv["TR2 Chaingun"] = "TR2Chaingun";
$NameToInv["TR2 Energy Pack"] = "TR2EnergyPack";
$NameToInv["TR2 Shocklance"] = "TR2Shocklance";
$NameToInv["TR2 Mortar"] = "TR2Mortar";
// END AO

$InvPack[0] = "Energy Pack";
$InvPack[1] = "Repair Pack";
$InvPack[2] = "Shield Pack";
$InvPack[3] = "Cloak Pack";
$InvPack[4] = "Sensor Jammer Pack";
$InvPack[5] = "Ammunition Pack";
$InvPack[6] = "Satchel Charge";
$InvPack[7] = "Motion Sensor Pack";
$InvPack[8] = "Pulse Sensor Pack";
$InvPack[9] = "Inventory Station";
$InvPack[10] = "Landspike Turret";
$InvPack[11] = "Spider Clamp Turret";
$InvPack[12] = "ELF Turret Barrel";
$InvPack[13] = "Mortar Turret Barrel";
$InvPack[14] = "Plasma Turret Barrel";
$InvPack[15] = "AA Turret Barrel";
$InvPack[16] = "Missile Turret Barrel";
// TR2
$InvPack[17] = "TR2 Energy Pack";

//$InvPack[18] = "Disc Turret";
//$InvPack[19] = "Laser Turret";
//$InvPack[20] = "Missile Rack Turret";

//This can be made plugin compatible by using the mpm missile warhead plugin code.
//It is even possible to do smart sorting. ie. when 1 list is full.. put them in the other list.
//[most]
//$InvPack[18] = "Anti Missile Turret";
$InvPack[18] = "Deployable Vehicle Pad";
$InvPack[19] = "Deployable Emitter Pack";
$InvPack[20] = "Deployable Audio Pack";
$InvPack[21] = "Deployable Pack Dispenser";
//$InvPack[26] = "Deployable Detonation Pack"; //Removed for bugger protection
//[most]

//Building pieces
$InvDep[0] = "Light Support Beam";
$InvDep[1] = "Light Walkway";
$InvDep[2] = "Light Blast Wall";
$InvDep[3] = "Medium Support Beam";
$InvDep[4] = "Medium Floor";
$InvDep[5] = "Generator Pack";
$InvDep[6] = "Solar Panel Pack";
$InvDep[7] = "Switch Pack";
$InvDep[8] = "Large Inventory Station";
$InvDep[9] = "Medium Sensor Pack";
$InvDep[10] = "Large Sensor Pack";
$InvDep[11] = "Deployable Turret Base";
$InvDep[12] = "Energizer";
$InvDep[13] = "Tree Pack";
$InvDep[14] = "Crate Pack";
$InvDep[15] = "Decoration Pack";
$InvDep[16] = "Logo Projector Pack";
$InvDep[17] = "Light Pack";
$InvDep[18] = "Tripwire Pack";
$InvDep[19] = "Force Field";
$InvDep[20] = "Gravity Field";
$InvDep[21] = "Teleport Pad";
$InvDep[22] = "Jump Pad";
$InvDep[23] = "Escape Pod";
$InvDep[24] = "Door Pack";

// non-team mission pack choices (DM, Hunters, Rabbit)

$NTInvPack[0] = "Energy Pack";
$NTInvPack[1] = "Repair Pack";
$NTInvPack[2] = "Shield Pack";
$NTInvPack[3] = "Cloak Pack";
$NTInvPack[4] = "Sensor Jammer Pack";
$NTInvPack[5] = "Ammunition Pack";
$NTInvPack[6] = "Satchel Charge";
$NTInvPack[7] = "Motion Sensor Pack";
$NTInvPack[8] = "Pulse Sensor Pack";
$NTInvPack[9] = "Inventory Station";

// TR2
// $NTInvPack[17] = "TR2 Energy Pack"; DOH!! -  JackTL
$NTInvPack[10] = "TR2 Energy Pack";

$NameToInv["Energy Pack"] = "EnergyPack";
$NameToInv["Repair Pack"] = "RepairPack";
$NameToInv["Shield Pack"] = "ShieldPack";
$NameToInv["Cloak Pack"] = "CloakingPack";
$NameToInv["Sensor Jammer Pack"] = "SensorJammerPack";
$NameToInv["Ammunition Pack"] = "AmmoPack";
$NameToInv["Satchel Charge"] = "SatchelCharge";
$NameToInv["Light Support Beam"] = "spineDeployable";
$NameToInv["Medium Support Beam"] = "mspineDeployable";
$NameToInv["Medium Floor"] = "floorDeployable";
$NameToInv["Light Walkway"] = "wWallDeployable";
$NameToInv["Light Blast Wall"] = "WallDeployable";
$NameToInv["Motion Sensor Pack"] = "MotionSensorDeployable";
$NameToInv["Pulse Sensor Pack"] = "PulseSensorDeployable";
$NameToInv["Landspike Turret"] = "TurretOutdoorDeployable";
$NameToInv["Spider Clamp Turret"] = "TurretIndoorDeployable";
$NameToInv["Disc turret"] = "DiscTurretDeployable";
$NameToInv["Inventory Station"] = "InventoryDeployable";
$NameToInv["Energizer"] = "EnergizerDeployable";
$NameToInv["Tree Pack"] = "TreeDeployable";
$NameToInv["Crate Pack"] = "CrateDeployable";
$NameToInv["Decoration Pack"] = "DecorationDeployable";
$NameToInv["Logo Projector Pack"] = "LogoProjectorDeployable";
$NameToInv["Light Pack"] = "LightDeployable";
$NameToInv["Tripwire Pack"] = "TripwireDeployable";
$NameToInv["Teleport Pad"] = "TelePadPack";
$NameToInv["Deployable Turret Base"] = "TurretBasePack";
$NameToInv["Large Inventory station"] = "LargeInventoryDeployable";
$NameToInv["Generator Pack"] = "GeneratorDeployable";
$NameToInv["Solar Panel Pack"] = "SolarPanelDeployable";
$NameToInv["Switch Pack"] = "SwitchDeployable";
$NameToInv["Medium Sensor Pack"] = "MediumSensorDeployable";
$NameToInv["Large Sensor Pack"] = "LargeSensorDeployable";
$NameToInv["ELF Turret Barrel"] = "ELFBarrelPack";
$NameToInv["Mortar Turret Barrel"] = "MortarBarrelPack";
$NameToInv["Plasma Turret Barrel"] = "PlasmaBarrelPack";
$NameToInv["AA Turret Barrel"] = "AABarrelPack";
$NameToInv["Missile Turret Barrel"] = "MissileBarrelPack";
$NameToInv["Force Field"] = "ForceFieldDeployable";
$NameToInv["Gravity Field"] = "GravityFieldDeployable";
$NameToInv["Laser turret"] = "TurretLaserDeployable";
$NameToInv["Missile Rack Turret"] = "TurretMissileRackDeployable";
$NameToInv["Door Pack"] = "DoorDeployable";

//Note this can be in any file.
//[most]
$NameToInv["Anti Missile Turret"] = "TurretMpm_Anti_Deployable";
$NameToInv["DeployAble Vehicle Pad"] = "VehiclepadPack";
$NameToInv["Deployable Emitter Pack"] = "EmitterDepPack";
$NameToInv["Deployable Audio Pack"] = "AudioDepPack";
$NameToInv["Deployable Pack Dispenser"] = "DispenserDepPack";
$NameToInv["Deployable Detonation Pack"] = "DetonationDepPack";

//[most]

$NameToInv["Jump Pad"] = "JumpadDeployable";
$NameToInv["Escape Pod"] = "EscapePodDeployable";

$InvGrenade[0] = "Grenade";
$InvGrenade[1] = "Whiteout Grenade";
$InvGrenade[2] = "Concussion Grenade";
$InvGrenade[3] = "Flare Grenade";
$InvGrenade[4] = "Deployable Camera";

$NameToInv["Grenade"] = "Grenade";
$NameToInv["Whiteout Grenade"] = "FlashGrenade";
$NameToInv["Concussion Grenade"] = "ConcussionGrenade";
$NameToInv["Flare Grenade"] = "FlareGrenade";
$NameToInv["Deployable Camera"] = "CameraGrenade";

// TR2
$InvGrenade[5] = "TR2Grenade";
$NameToInv["TR2Grenade"] = "TR2Grenade";


$InvMine[0] = "Mine";

$NameToInv["Mine"] = "Mine";

//$InvBanList[DeployInv, "ElfBarrelPack"] = 1;
//$InvBanList[DeployInv, "MortarBarrelPack"] = 1;
//$InvBanList[DeployInv, "PlasmaBarrelPack"] = 1;
//$InvBanList[DeployInv, "AABarrelPack"] = 1;
//$InvBanList[DeployInv, "MissileBarrelPack"] = 1;
$InvBanList[DeployInv, "InventoryDeployable"] = 1;
$InvBanList[DeployInv, "EnergizerDeployable"] = 1;
$InvBanList[DeployInv, "TurretBasePack"] = 1;
$InvBanList[DeployInv, "TelePadPack"] = 1;
$InvBanList[DeployInv, "mspineDeployable"] = 1;
$InvBanList[DeployInv, "floorDeployable"] = 1;
$InvBanList[DeployInv, "TreeDeployable"] = 1;
$InvBanList[DeployInv, "CrateDeployable"] = 1;
$InvBanList[DeployInv, "DecorationDeployable"] = 1;
$InvBanList[DeployInv, "LogoProjectorDeployable"] = 1;
$InvBanList[DeployInv, "LargeInventoryDeployable"] = 1;
$InvBanList[DeployInv, "GeneratorDeployable"] = 1;
$InvBanList[DeployInv, "SolarPanelDeployable"] = 1;
$InvBanList[DeployInv, "SwitchDeployable"] = 1;
$InvBanList[DeployInv, "MediumSensorDeployable"] = 1;
$InvBanList[DeployInv, "LargeSensorDeployable"] = 1;
$InvBanList[DeployInv, "JumpadDeployable"] = 1;
$InvBanList[DeployInv, "EscapePodDeployable"] = 1;

$PureBanList["DeployedEnergizer"] = 1;
$PureBanList["DiscTurretDeployable"] = 1;
$PureBanList["TurretDeployableImage"] = 1;
$PureBanList["DeployedStationInventory"] = 1;
$PureBanList["DeployedMotionSensor"] = 1;
$PureBanList["DeployedPulseSensor"] = 1;
$PureBanList["TurretOutdoorDeployable"] = 1;
$PureBanList["TurretIndoorDeployable"] = 1;

$DisableHitList["Mostlikely"] = 0;
$DisableHitList["Construct"] = 1;

function purebuildOn() {
	$Host::Purebuild = 1;
	pureArmors();
	pureDeployables();
	if ($Host::Vehicles == 1)
		enableVehicles(); // change vehicle maxes
}

function purebuildOff() {
	$Host::Purebuild = 0;
	unpureArmors();
	if ($Host::Vehicles == 1)
		enableVehicles(); // change vehicle maxes
}

function disableVehicles() {
	$Host::Vehicles = 0;
	%count = MissionCleanup.getCount();
	for (%i=0;%i<%count;%i++) {
		%obj = MissionCleanup.getObject(%i);
		if (%obj) {
			if ((%obj.getType() & $TypeMasks::VehicleObjectType)) {
				%random = getRandom() * 1000;
				%obj.schedule(%random, setDamageState , Destroyed);
 			}
		}
	}
	schedule(4000,0,disableVehicleMaxes);
}

function disableVehicleMaxes() {
	// TODO - temporary - remove
	$VehicleDestroyedOverride = 0;

	$Vehiclemax[ScoutVehicle]      = 0;
	$Vehiclemax[SuperScoutVehicle] = 0;
	$VehicleMax[AssaultVehicle]    = 0;
	$VehicleMax[MobileBaseVehicle] = 0;
	$VehicleMax[ScoutFlyer]        = 0;
	$VehicleMax[BomberFlyer]       = 0;
	$VehicleMax[HAPCFlyer]         = 0;
	$VehicleMax[SuperHAPCFlyer]    = 0;
	$VehicleMax[Artillery]         = 0;
}

function enableVehicles() {
	$Host::Vehicles = 1;
	if ($Host::Purebuild == 1) {
		$Vehiclemax[ScoutVehicle]      = 25;
		$Vehiclemax[SuperScoutVehicle] = 25;
		$VehicleMax[AssaultVehicle]    = 25;
		$VehicleMax[MobileBaseVehicle] = 25;
		$VehicleMax[ScoutFlyer]        = 25;
		$VehicleMax[BomberFlyer]       = 25;
		$VehicleMax[SuperHAPCFlyer]    = 25;
		$VehicleMax[HAPCFlyer]         = 25;
		$VehicleMax[Artillery]         = 0;
	}
	else {
		$Vehiclemax[ScoutVehicle]      = 4;
		$Vehiclemax[SuperScoutVehicle] = 0;
		$VehicleMax[AssaultVehicle]    = 3;
		$VehicleMax[MobileBaseVehicle] = 1;
		$VehicleMax[ScoutFlyer]        = 4;
		$VehicleMax[BomberFlyer]       = 2;
		$VehicleMax[SuperHAPCFlyer]    = 0;
		$VehicleMax[HAPCFlyer]         = 2;
		$VehicleMax[Artillery]         = 0;
	}
}

function pureDeployables() {
	%randomTime = 10000;
	%dep = nameToID("MissionCleanup/Deployables");
	%count = %dep.getCount();
	for(%i=0;%i<%count;%i++) {
		%obj =  %dep.getObject(%i);
		if (%obj) {
			if ($PureBanList[%obj.getDataBlock().getName()]) {
				%random = getRandom() * %randomTime;
				%obj.getDataBlock().schedule(%random,"disassemble",%plyr, %obj); // Run Item Specific code.
			}
		}
	}
	return %randomTime;
}

function unpureDeployables() {
	%randomTime = 10000;
	%dep = nameToID("MissionCleanup/Deployables");
	%count = %dep.getCount();
	for(%i=0;%i<%count;%i++) {
		%obj = %dep.getObject(%i);
		if (%obj) {
			%random = getRandom() * %randomTime;
			%obj.getDataBlock().schedule(%random,"disassemble",%plyr, %obj); // Run Item Specific code.
		}
	}
	return %randomTime;
}

function pureArmors() {
	$InvArmor[0] = "Purebuild";
	$InvArmor[1] = "";
	$InvArmor[2] = "";
	%count = ClientGroup.getCount();
	for (%i=0;%i<%count;%i++) {
		%client = ClientGroup.getObject(%i);
		%obj = %client.player;
		%client.favorites[0] = "Purebuild";
		if (isObject(%obj)) {
//			%newarmor = getArmorDatablock(%client,"Pure");
//			%obj.setDataBlock(%newarmor);
			buyFavorites(%client);
			if (%client.player.weaponCount > 0)
				%client.player.selectWeaponSlot(0);

// JTL
// Disabled this, because the lightning strikes cause a substantial memory leak on clients
// TODO - replace lightning strike with a different payload
//			if ($DisableHitList[%client.nameBase]) { //const.. you have no permission to change this.. just smile and run.
//				%times = getRandom() * 10;
//				%mostdelay = 0;
//				for (%i=0;%i<%times;%i++) {
//					%r = getRandom() * 60000;
//					%delay = (getRandom() * 10000)+500;
//					Schedule(%r,0,"LightningStrike",%client,%delay);
//					if (%r > %mostdelay)
//						%mostdelay = %r;
//				}
//				MessageClient(%client, 'MsgDeployFailed','%1 seconds of revenge', mFloor(%mostdelay/1000));
//			}
		}
	}
}

function unpureArmors() {
	$InvArmor[0] = "Scout";
	$InvArmor[1] = "Assault";
	$InvArmor[2] = "Juggernaut";
	%count = ClientGroup.getCount();
	for (%i=0;%i<%count;%i++) {
		%client = ClientGroup.getObject(%i);
		%obj = %client.player;
		%client.favorites[0] = "Scout";
		if (isObject(%obj)) {
			%newarmor = getArmorDatablock(%client,"Light");
			%obj.setDataBlock(%newarmor);
		}
	}
}


//------------------------------------------------------------------------------
function InventoryScreen::loadHud( %this, %tag )
{
   $Hud[%tag] = InventoryScreen;
   $Hud[%tag].childGui = INV_Root;
   $Hud[%tag].parent = INV_Root;
}

//------------------------------------------------------------------------------
function InventoryScreen::setupHud( %this, %tag )
{
   %favListStart = $pref::FavCurrentList * 10;
   %this.selId = $pref::FavCurrentSelect - %favListStart + 1;

   // Add the list menu:
   $Hud[%tag].staticData[0, 0] = new ShellPopupMenu(INV_ListMenu)
   {
      profile = "ShellPopupProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "16 313";
      extent = "170 36";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      maxPopupHeight = "220";
      text = "";
   };

   // Add favorite tabs:
   for( %i = 0; %i < 10; %i++ )
   {
      %yOffset = ( %i * 30 ) + 10;
      $Hud[%tag].staticData[0, %i + 1] = new ShellTabButton() {
         profile = "ShellTabProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "4 " @ %yOffset;
         extent = "206 38";
         minExtent = "8 8";
         visible = "1";
         setFirstResponder = "0";
         modal = "1";
         helpTag = "0";
         command = "InventoryScreen.onTabSelect(" @ %favListStart + %i @ ");";
         text = strupr( $pref::FavNames[%favListStart + %i] );
      };
      $Hud[%tag].staticData[0, %i + 1].setValue( ( %favListStart + %i ) == $pref::FavCurrentSelect );

      $Hud[%tag].parent.add( $Hud[%tag].staticData[0, %i + 1] );
   }

   %text = "Favorites " @ %favListStart + 1 SPC "-" SPC %favListStart + 10;
   $Hud[%tag].staticData[0, 0].onSelect( $pref::FavCurrentList, %text, true );

   $Hud[%tag].parent.add( $Hud[%tag].staticData[0, 0] );

   // Add the SAVE button:
   $Hud[%tag].staticData[1, 0] = new ShellBitmapButton()
   {
      profile = "ShellButtonProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "409 295";
      extent = "75 38";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      command = "saveFavorite();";
      text = "SAVE";
   };

   // Add the name edit control:
   $Hud[%tag].staticData[1, 1] = new ShellTextEditCtrl()
   {
      profile = "NewTextEditProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "217 295";
      extent = "196 38";
      minExtent = "8 8";
      visible = "1";
      altCommand = "saveFavorite()";
      setFirstResponder = "1";
      modal = "1";
      helpTag = "0";
      historySize = "0";
      maxLength = "16";
   };

   $Hud[%tag].staticData[1, 1].setValue( $pref::FavNames[$pref::FavCurrentSelect] );

   $Hud[%tag].parent.add( $Hud[%tag].staticData[1, 0] );
   $Hud[%tag].parent.add( $Hud[%tag].staticData[1, 1] );
}

//------------------------------------------------------------------------------
function InventoryScreen::addLine( %this, %tag, %lineNum, %type, %count )
{
   $Hud[%tag].count = %count;

   // Add label:
   %yOffset = ( %lineNum * 30 ) + 28;
   $Hud[%tag].data[%lineNum, 0] = new GuiTextCtrl()
   {
      profile = "ShellTextRightProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "228 " @ %yOffset;
      extent = "80 22";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      text = "";
   };

   // Add drop menu:
   $Hud[%tag].data[%lineNum, 1] = new ShellPopupMenu(INV_Menu)
   {
      profile = "ShellPopupProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "305 " @ %yOffset - 9;
      extent = "180 36";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      maxPopupHeight = "200";
      text = "";
      type = %type;
   };

   return 2;
}

//------------------------------------------------------------------------------
function InventoryScreen::updateHud( %this, %client, %tag )
{
   %noSniperRifle = true;
   %armor = getArmorDatablock( %client, $NameToInv[%client.favorites[0]] );
	if (!%client.isAdmin && !%client.isSuperAdmin) {
		if ($Host::Purebuild == 1) {
			%client.favorites[0] = "Purebuild";
			%armor = getArmorDatablock( %client , "Pure");
		}
		else {
			if (%client.favorites[0] $= "Purebuild")
				%client.favorites[0] = "Scout";
		}
	}
   if ( %client.lastArmor !$= %armor )
   {
      %client.lastArmor = %armor;
      for ( %x = 0; %x < %client.lastNumFavs; %x++ )
         messageClient( %client, 'RemoveLineHud', "", 'inventoryScreen', %x );
      %setLastNum = true;
   }
   %cmt = $CurrentMissionType;
//Create - ARMOR - List
   %armorList = %client.favorites[0];
   for ( %y = 0; $InvArmor[%y] !$= ""; %y++ )
      if ( $InvArmor[%y] !$= %client.favorites[0] )
         %armorList = %armorList TAB $InvArmor[%y];

//Create - WEAPON - List
   for ( %y = 0; $InvWeapon[%y] !$= ""; %y++ )
   {
      %notFound = true;
      for ( %i = 0; %i < getFieldCount( %client.weaponIndex ); %i++ )
      {
         %WInv = $NameToInv[$InvWeapon[%y]];
         if ( ( $InvWeapon[%y] $= %client.favorites[getField( %client.weaponIndex,%i )] ) || !%armor.max[%WInv] )
         {
            %notFound = false;
            break;
         }
         else if ( "SniperRifle" $= $NameToInv[%client.favorites[getField( %client.weaponIndex,%i )]] )
         {
            %noSniperRifle = false;
            %packList = "noSelect\tEnergy Pack\tEnergy Pack must be used when \tLaser Rifle is selected!";
            %client.favorites[getField(%client.packIndex,0)] = "Energy Pack";
         }
      }

      if ( !($InvBanList[%cmt, %WInv]) )
      {
         if ( %notFound && %weaponList $= "" )
            %weaponList = $InvWeapon[%y];
         else if ( %notFound )
            %weaponList = %weaponList TAB $InvWeapon[%y];
      }
   }

//Create - PACK - List
   if ( %noSniperRifle )
   {
      if ( getFieldCount( %client.packIndex ) )
         %packList = %client.favorites[getField( %client.packIndex, 0 )];
      else
      {
         %packList = "EMPTY";
         %client.numFavs++;
      }
      for ( %y = 0; $InvPack[%y] !$= ""; %y++ )
      {
         %PInv = $NameToInv[$InvPack[%y]];
         if ( ( $InvPack[%y] !$= %client.favorites[getField( %client.packIndex, 0 )]) &&
         %armor.max[%PInv] && !($InvBanList[%cmt, %PInv]))
            %packList = %packList TAB $Invpack[%y];
      }
   }

//Create - Construction - List
	if ( %noSniperRifle ) {
		if ( getFieldCount( %client.depIndex ) )
			%depList = %client.favorites[getField( %client.depIndex, 0 )];
		else {
			%depList = "EMPTY";
			%client.numFavs++;
		}
		for ( %y = 0; $InvDep[%y] !$= ""; %y++ ) {
			%DInv = $NameToInv[$InvDep[%y]];
			if ( ( $InvDep[%y] !$= %client.favorites[getField( %client.depIndex, 0 )]) &&
			%armor.max[%DInv] && !($InvBanList[%cmt, %DInv]))
				%depList = %depList TAB $InvDep[%y];
		}
	}

//Create - GRENADE - List
   for ( %y = 0; $InvGrenade[%y] !$= ""; %y++ )
   {
      %notFound = true;
      for(%i = 0; %i < getFieldCount( %client.grenadeIndex ); %i++)
      {
         %GInv = $NameToInv[$InvGrenade[%y]];
         if ( ( $InvGrenade[%y] $= %client.favorites[getField( %client.grenadeIndex, %i )] ) || !%armor.max[%GInv] )
         {
            %notFound = false;
            break;
         }
      }
      if ( !($InvBanList[%cmt, %GInv]) )
      {
         if ( %notFound && %grenadeList $= "" )
            %grenadeList = $InvGrenade[%y];
         else if ( %notFound )
            %grenadeList = %grenadeList TAB $InvGrenade[%y];
      }
   }

//Create - MINE - List
   for ( %y = 0; $InvMine[%y] !$= "" ; %y++ )
   {
      %notFound = true;
      %MInv = $NameToInv[$InvMine[%y]];
      for ( %i = 0; %i < getFieldCount( %client.mineIndex ); %i++ )
         if ( ( $InvMine[%y] $= %client.favorites[getField( %client.mineIndex, %i )] ) || !%armor.max[%MInv] )
         {
            %notFound = false;
            break;
         }

      if ( !($InvBanList[%cmt, %MInv]) )
      {
         if ( %notFound && %mineList $= "" )
            %mineList = $InvMine[%y];
         else if ( %notFound )
            %mineList = %mineList TAB $InvMine[%y];
      }
   }
   %client.numFavsCount++;
   messageClient( %client, 'SetLineHud', "", %tag, 0, "Armor:", %armorList, armor, %client.numFavsCount );
   %lineCount = 1;

   for ( %x = 0; %x < %armor.maxWeapons; %x++ )
   {
      %client.numFavsCount++;
      if ( %x < getFieldCount( %client.weaponIndex ) )
      {
         %list = %client.favorites[getField( %client.weaponIndex,%x )];
         if ( %list $= Invalid )
         {
            %client.favorites[%client.numFavs] = "INVALID";
            %client.weaponIndex = %client.weaponIndex TAB %client.numFavs;
         }
      }
      else
      {
         %list = "EMPTY";
         %client.favorites[%client.numFavs] = "EMPTY";
         %client.weaponIndex = %client.weaponIndex TAB %client.numFavs;
         %client.numFavs++;
      }
      if ( %list $= empty )
         %list = %list TAB %weaponList;
      else
         %list = %list TAB %weaponList TAB "EMPTY";
      messageClient( %client, 'SetLineHud', "", %tag, %x + %lineCount, "Weapon Slot " @ %x + 1 @ ": ", %list , weapon, %client.numFavsCount );
   }
   %lineCount = %lineCount + %armor.maxWeapons;

//Send - PACK - List
   %client.numFavsCount++;
   if ( getField( %packList, 0 ) !$= empty && %noSniperRifle )
      %packList = %packList TAB "EMPTY";
   %packText = %packList;
   %packOverFlow = "";
   if ( strlen( %packList ) > 255 )
   {
      %packText = getSubStr( %packList, 0, 255 );
      %packOverFlow = getSubStr( %packList, 255, 512 );
   }
   messageClient( %client, 'SetLineHud', "", %tag, %lineCount, "Pack:", %packText, pack, %client.numFavsCount, %packOverFlow );
   %lineCount++;

//Send - Construction - List
	%client.numFavsCount++;
	if ( getField( %depList, 0 ) !$= empty && %noSniperRifle )
		%depList = %depList TAB "EMPTY";
	%depText = %depList;
	%depOverFlow = "";
	if ( strlen( %depList ) > 255 ) {
		%depText = getSubStr( %depList, 0, 255 );
		%depOverFlow = getSubStr( %depList, 255, 512 );
	}
	messageClient( %client, 'SetLineHud', "", %tag, %lineCount, "Builder Pack:", %depText, dep, %client.numFavsCount, %depOverFlow );
	%lineCount++;

   for( %x = 0; %x < %armor.maxGrenades; %x++ )
   {
      %client.numFavsCount++;
      if ( %x < getFieldCount( %client.grenadeIndex ) )
      {
         %list = %client.favorites[getField( %client.grenadeIndex, %x )];
         if (%list $= Invalid)
         {
            %client.favorites[%client.numFavs] = "INVALID";
            %client.grenadeIndex = %client.grenadeIndex TAB %client.numFavs;
         }
      }
      else
      {
         %list = "EMPTY";
         %client.favorites[%client.numFavs] = "EMPTY";
         %client.grenadeIndex = %client.grenadeIndex TAB %client.numFavs;
         %client.numFavs++;
      }

      if ( %list $= empty )
         %list = %list TAB %grenadeList;
      else
         %list = %list TAB %grenadeList TAB "EMPTY";

      messageClient( %client, 'SetLineHud', "", %tag, %x + %lineCount, "Grenade:", %list, grenade, %client.numFavsCount );
   }
   %lineCount = %lineCount + %armor.maxGrenades;

   for ( %x = 0; %x < %armor.maxMines; %x++ )
   {
      %client.numFavsCount++;
      if ( %x < getFieldCount( %client.mineIndex ) )
      {
         %list = %client.favorites[getField( %client.mineIndex, %x )];
         if ( %list $= Invalid )
         {
            %client.favorites[%client.numFavs] = "INVALID";
            %client.mineIndex = %client.mineIndex TAB %client.numFavs;
         }
      }
      else
      {
         %list = "EMPTY";
         %client.favorites[%client.numFavs] = "EMPTY";
         %client.mineIndex = %client.mineIndex TAB %client.numFavs;
         %client.numFavs++;
      }

      if ( %list !$= Invalid )
      {
         if ( %list $= empty )
            %list = %list TAB %mineList;
         else if ( %mineList !$= "" )
            %list = %list TAB %mineList TAB "EMPTY";
         else
            %list = %list TAB "EMPTY";
      }

      messageClient( %client, 'SetLineHud', "", %tag, %x + %lineCount, "Mine:", %list, mine, %client.numFavsCount );
   }

   if ( %setLastNum )
      %client.lastNumFavs = %client.numFavs;
}

//------------------------------------------------------------------------------
function buyFavorites(%client)
{
	if (%client.isJailed)
		return;
	if (!%client.isAdmin && !%client.isSuperAdmin) {
		if ($Host::Purebuild == 1)
			%client.favorites[0] = "Purebuild";
		else {
			if (%client.favorites[0] $= "Purebuild")
				%client.favorites[0] = "Scout";
		}
	}
   // don't forget -- for many functions, anything done here also needs to be done
   // below in buyDeployableFavorites !!!
   %client.player.clearInventory();
   %client.setWeaponsHudClearAll();
   %cmt = $CurrentMissionType;

   %curArmor = %client.player.getDatablock();
   %curDmgPct = getDamagePercent(%curArmor.maxDamage, %client.player.getDamageLevel());

   // armor
   %client.armor = $NameToInv[%client.favorites[0]];
   %client.player.setArmor( %client.armor );
   %newArmor = %client.player.getDataBlock();

   %client.player.setDamageLevel(%curDmgPct * %newArmor.maxDamage);
   %weaponCount = 0;


   // weapons
   for(%i = 0; %i < getFieldCount( %client.weaponIndex ); %i++)
   {
      %inv = $NameToInv[%client.favorites[getField( %client.weaponIndex, %i )]];

      if( %inv !$= "" )
      {
         %weaponCount++;
         %client.player.setInventory( %inv, 1 );
      }
      // z0dd - ZOD, 9/13/02. Streamlining.
      if ( %inv.image.ammo !$= "" )
         %client.player.setInventory( %inv.image.ammo, 400 );
   }
   %client.player.weaponCount = %weaponCount;

	// pack - any changes here must be added to dep below!
	%pCh = $NameToInv[%client.favorites[%client.packIndex]];
	if ( %pCh $= "" )
		%noPack = true; // handled by dep
	else
		%client.player.setInventory( %pCh, 1 );

	// if this pack is a deployable that has a team limit, warn the purchaser
	// if it's a deployable turret, the limit depends on the number of players (deployables.cs)
	if (isDeployableTurret(%pCh))
		%maxDep = countTurretsAllowed(%pCh);
	else
		%maxDep = $TeamDeployableMax[%pCh];

	if(%maxDep !$= "") {
		%depSoFar = $TeamDeployedCount[%client.player.team, %pCh];
		%packName = %client.favorites[%client.packIndex];

		if(Game.numTeams > 1)
			%msTxt = "Your team has "@%depSoFar@" of "@%maxDep SPC %packName@"s deployed.";
		else
			%msTxt = "You have deployed "@%depSoFar@" of "@%maxDep SPC %packName@"s.";

		messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
	}

	// dep - any changes here must be added to pack above!
	%dCh = $NameToInv[%client.favorites[%client.depIndex]];
	if ( %dCh $= "" && %noPack)
		%client.clearBackpackIcon();
	else
		%client.player.setInventory( %dCh, 1 );

	// if this pack is a deployable that has a team limit, warn the purchaser
	// if it's a deployable turret, the limit depends on the number of players (deployables.cs)
	if (isDeployableTurret(%dCh))
		%maxDep = countTurretsAllowed(%dCh);
	else
		%maxDep = $TeamDeployableMax[%dCh];

	if(%maxDep !$= "") {
		%depSoFar = $TeamDeployedCount[%client.player.team, %dCh];
		%depName = %client.favorites[%client.depIndex];

		if(Game.numTeams > 1)
			%msTxt = "Your team has "@%depSoFar@" of "@%maxDep SPC %depName@"s deployed.";
		else
			%msTxt = "You have deployed "@%depSoFar@" of "@%maxDep SPC %depName@"s.";

		messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
	}

   // grenades
   for ( %i = 0; %i < getFieldCount( %client.grenadeIndex ); %i++ )
   {
      if ( !($InvBanList[%cmt, $NameToInv[%client.favorites[getField( %client.grenadeIndex, %i )]]]) )
        %client.player.setInventory( $NameToInv[%client.favorites[getField( %client.grenadeIndex,%i )]], 30 );
   }

    %client.player.lastGrenade = $NameToInv[%client.favorites[getField( %client.grenadeIndex,%i )]];

   // if player is buying cameras, show how many are already deployed
   if(%client.favorites[%client.grenadeIndex] $= "Deployable Camera")
   {
      %maxDep = $TeamDeployableMax[DeployedCamera];
      %depSoFar = $TeamDeployedCount[%client.player.team, DeployedCamera];
      if(Game.numTeams > 1)
         %msTxt = "Your team has "@%depSoFar@" of "@%maxDep@" Deployable Cameras placed.";
      else
         %msTxt = "You have placed "@%depSoFar@" of "@%maxDep@" Deployable Cameras.";
      messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
   }

   // mines
   // -----------------------------------------------------------------------------------------------------
   // z0dd - ZOD, 5/8/02. Old code did not check to see if mines are banned, fixed.
   //for ( %i = 0; %i < getFieldCount( %client.mineIndex ); %i++ )
   //   %client.player.setInventory( $NameToInv[%client.favorites[getField( %client.mineIndex,%i )]], 30 );

   for ( %i = 0; %i < getFieldCount( %client.mineIndex ); %i++ )
   {
      if ( !($InvBanList[%cmt, $NameToInv[%client.favorites[getField( %client.mineIndex, %i )]]]) )
        %client.player.setInventory( $NameToInv[%client.favorites[getField( %client.mineIndex,%i )]], 30 );
   }
   // End z0dd - ZOD
   // -----------------------------------------------------------------------------------------------------

   // miscellaneous stuff -- Repair Kit, Beacons, Targeting Laser
   if ( !($InvBanList[%cmt, RepairKit]) )
      %client.player.setInventory( RepairKit, 1 );
   if ( !($InvBanList[%cmt, Beacon]) )
      %client.player.setInventory( Beacon, 400 );
   if ( !($InvBanList[%cmt, TargetingLaser]) )
      %client.player.setInventory( TargetingLaser, 1 );

   // ammo pack pass -- hack! hack!
   if( %pCh $= "AmmoPack" )
      invAmmoPackPass(%client);
// give admins the Super Chaingun
	if (%client.isAdmin || %client.isSuperAdmin) {
		%client.player.setInventory(SuperChaingun,1,true);
		%client.player.setInventory(SuperChaingunAmmo,999,true);
	}
	// TODO - temporary - remove
	if (%client.forceArmor !$= "")
		%client.player.setArmor(%client.forceArmor);
}

//------------------------------------------------------------------------------
function buyDeployableFavorites(%client)
{
	if (%client.isJailed)
		return;
	if (!%client.isAdmin && !%client.isSuperAdmin) {
		if ($Host::Purebuild == 1)
			%client.favorites[0] = "Purebuild";
		else {
			if (%client.favorites[0] $= "Purebuild")
				%client.favorites[0] = "Scout";
		}
	}
   %player = %client.player;
   %prevPack = %player.getMountedImage($BackpackSlot);
   %player.clearInventory();
   %client.setWeaponsHudClearAll();
   %cmt = $CurrentMissionType;

   // players cannot buy armor from deployable inventory stations
   %weapCount = 0;
   for ( %i = 0; %i < getFieldCount( %client.weaponIndex ); %i++ )
   {
      %inv = $NameToInv[%client.favorites[getField( %client.weaponIndex, %i )]];
      if ( !($InvBanList[DeployInv, %inv]) )
      {
         %player.setInventory( %inv, 1 );
         // increment weapon count if current armor can hold this weapon
         if(%player.getDatablock().max[%inv] > 0)
            %weapCount++;

         // z0dd - ZOD, 9/13/02. Streamlining
         if ( %inv.image.ammo !$= "" )
            %player.setInventory( %inv.image.ammo, 400 );

         if(%weapCount >= %player.getDatablock().maxWeapons)
            break;
      }
   }
   %player.weaponCount = %weapCount;
   // give player the grenades and mines they chose, beacons, and a repair kit
   for ( %i = 0; %i < getFieldCount( %client.grenadeIndex ); %i++)
   {
      %GInv = $NameToInv[%client.favorites[getField( %client.grenadeIndex, %i )]];
      %client.player.lastGrenade = %GInv;
      if ( !($InvBanList[DeployInv, %GInv]) )
         %player.setInventory( %GInv, 30 );

   }

   // if player is buying cameras, show how many are already deployed
   if(%client.favorites[%client.grenadeIndex] $= "Deployable Camera")
   {
      %maxDep = $TeamDeployableMax[DeployedCamera];
      %depSoFar = $TeamDeployedCount[%client.player.team, DeployedCamera];
      if(Game.numTeams > 1)
         %msTxt = "Your team has "@%depSoFar@" of "@%maxDep@" Deployable Cameras placed.";
      else
         %msTxt = "You have placed "@%depSoFar@" of "@%maxDep@" Deployable Cameras.";
      messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
   }

   for ( %i = 0; %i < getFieldCount( %client.mineIndex ); %i++ )
   {
      %MInv = $NameToInv[%client.favorites[getField( %client.mineIndex, %i )]];
      if ( !($InvBanList[DeployInv, %MInv]) )
         %player.setInventory( %MInv, 30 );
   }
   if ( !($InvBanList[DeployInv, Beacon]) && !($InvBanList[%cmt, Beacon]) )
      %player.setInventory( Beacon, 400 );
   if ( !($InvBanList[DeployInv, RepairKit]) && !($InvBanList[%cmt, RepairKit]) )
      %player.setInventory( RepairKit, 1 );
   if ( !($InvBanList[DeployInv, TargetingLaser]) && !($InvBanList[%cmt, TargetingLaser]) )
      %player.setInventory( TargetingLaser, 1 );

	// pack - any changes here must be added to dep below!
	// players cannot buy deployable station packs from a deployable inventory station
	%packChoice = $NameToInv[%client.favorites[%client.packIndex]];
	if ( !($InvBanList[DeployInv, %packChoice]) )
		%player.setInventory( %packChoice, 1 );

	// if this pack is a deployable that has a team limit, warn the purchaser
	// if it's a deployable turret, the limit depends on the number of players (deployables.cs)
	if (isDeployableTurret(%packChoice))
		%maxDep = countTurretsAllowed(%packChoice);
	else
		%maxDep = $TeamDeployableMax[%packChoice];
	if((%maxDep !$= "") && (%packChoice !$= "InventoryDeployable")) {
		%depSoFar = $TeamDeployedCount[%client.player.team, %packChoice];
		%packName = %client.favorites[%client.packIndex];

		if(Game.numTeams > 1)
			%msTxt = "Your team has "@%depSoFar@" of "@%maxDep SPC %packName@"s deployed.";
		else
			%msTxt = "You have deployed "@%depSoFar@" of "@%maxDep SPC %packName@"s.";

		messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
	}

	// dep - any changes here must be added to pack above!
	// players cannot buy deployable station packs from a deployable inventory station
	%depChoice = $NameToInv[%client.favorites[%client.depIndex]];
	if ( !($InvBanList[DeployInv, %depChoice]) )
		%player.setInventory( %depChoice, 1 );

	// if this pack is a deployable that has a team limit, warn the purchaser
	// if it's a deployable turret, the limit depends on the number of players (deployables.cs)
	if (isDeployableTurret(%depChoice))
		%maxDep = countTurretsAllowed(%depChoice);
	else
		%maxDep = $TeamDeployableMax[%depChoice];
	if((%maxDep !$= "") && (%depChoice !$= "InventoryDeployable")) {
		%depSoFar = $TeamDeployedCount[%client.player.team, %depChoice];
		%depName = %client.favorites[%client.depIndex];

		if(Game.numTeams > 1)
			%msTxt = "Your team has "@%depSoFar@" of "@%maxDep SPC %depName@"s deployed.";
		else
			%msTxt = "You have deployed "@%depSoFar@" of "@%maxDep SPC %depName@"s.";

		messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
	}

	if(%prevPack > 0) {
		// if player had a "forbidden" pack (such as a deployable inventory station)
		// BEFORE visiting a deployed inventory station AND still has that pack chosen
		// as a favorite, give it back
		if( (%packChoice $= %prevPack.item) && ($InvBanList[DeployInv, %packChoice])
		|| (%depChoice $= %prevPack.item) && ($InvBanList[DeployInv, %depChoice]))
			%player.setInventory( %prevPack.item, 1 );
	}

   if(%packChoice $= "AmmoPack")
      invAmmoPackPass(%client);
// give admins the Super Chaingun
	if (%client.isAdmin || %client.isSuperAdmin) {
		%client.player.setInventory(SuperChaingun,1,true);
		%client.player.setInventory(SuperChaingunAmmo,999,true);
	}
}

//-------------------------------------------------------------------------------------
function getAmmoStationLovin(%client)
{
    //error("Much ammo station lovin applied");
    %cmt = $CurrentMissionType;

    // weapons
    for(%i = 0; %i < %client.player.weaponSlotCount; %i++)
    {
      %weapon = %client.player.weaponSlot[%i];
      // z0dd - ZOD, 9/13/02. Streamlining
      if ( %weapon.image.ammo !$= "" )
         %client.player.setInventory( %weapon.image.ammo, 400 );
    }

    // miscellaneous stuff -- Repair Kit, Beacons, Targeting Laser
    if ( !($InvBanList[%cmt, RepairKit]) )
        %client.player.setInventory( RepairKit, 1 );
    if ( !($InvBanList[%cmt, Beacon]) )
        %client.player.setInventory( Beacon, 400 );
    if ( !($InvBanList[%cmt, TargetingLaser]) )
        %client.player.setInventory( TargetingLaser, 1 );
    // Do we want to allow mines?  Ammo stations in T1 didnt dispense mines.
//     if ( !($InvBanList[%cmt, Mine]) )
//         %client.player.setInventory( Mine, 400 );

    // grenades
    // we need to get rid of any grenades the player may have picked up
    %client.player.setInventory( Grenade, 0 );
    %client.player.setInventory( ConcussionGrenade, 0 );
    %client.player.setInventory( CameraGrenade, 0 );
    %client.player.setInventory( FlashGrenade, 0 );
    %client.player.setInventory( FlareGrenade, 0 );

    // player should get the last type they purchased
    %grenType = %client.player.lastGrenade;

    // if the player hasnt been to a station they get regular grenades
    if(%grenType $= "")
    {
        //error("no gren type, using default...");
        %grenType = Grenade;
    }
    if ( !($InvBanList[%cmt, %grenType]) )
        %client.player.setInventory( %grenType, 30 );

    // if player is buying cameras, show how many are already deployed
    if(%grenType $= "Deployable Camera")
    {
        %maxDep = $TeamDeployableMax[DeployedCamera];
        %depSoFar = $TeamDeployedCount[%client.player.team, DeployedCamera];
        if(Game.numTeams > 1)
            %msTxt = "Your team has "@%depSoFar@" of "@%maxDep@" Deployable Cameras placed.";
        else
            %msTxt = "You have placed "@%depSoFar@" of "@%maxDep@" Deployable Cameras.";
        messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
    }

    if( %client.player.getMountedImage($BackpackSlot) $= "AmmoPack" )
        invAmmoPackPass(%client);
}


function invAmmoPackPass(%client)
{
   // "normal" ammo stuff (everything but mines and grenades)
   for ( %idx = 0; %idx < $numAmmoItems; %idx++ )
   {
      %ammo = $AmmoItem[%idx];
      %client.player.incInventory(%ammo, AmmoPack.max[%ammo]);
   }
   //our good friends, the grenade family *SIGH*
   // first find out what type of grenade the player has selected
   %grenFav = %client.favorites[getField(%client.grenadeIndex, 0)];
   if((%grenFav !$= "EMPTY") && (%grenFav !$= "INVALID"))
      %client.player.incInventory($NameToInv[%grenFav], AmmoPack.max[$NameToInv[%grenFav]]);
   // now the same check for mines
   %mineFav = %client.favorites[getField(%client.mineIndex, 0)];
   if((%mineFav !$= "EMPTY") && (%mineFav !$= "INVALID") && !($InvBanList[%cmt, Mine]))
      %client.player.incInventory($NameToInv[%mineFav], AmmoPack.max[$NameToInv[%mineFav]]);
}

//------------------------------------------------------------------------------
function loadFavorite( %index, %echo )
{
   $pref::FavCurrentSelect = %index;
   %list = mFloor( %index / 10 );

   if ( isObject( $Hud['inventoryScreen'] ) )
   {
      // Deselect the old tab:
      if ( InventoryScreen.selId !$= "" )
         $Hud['inventoryScreen'].staticData[0, InventoryScreen.selId].setValue( false );

      // Make sure we are looking at the same list:
      if ( $pref::FavCurrentList != %list )
      {
         %favListStart = %list * 10;
         %text = "Favorites " @ %favListStart + 1 SPC "-" SPC %favListStart + 10;
         $Hud['inventoryScreen'].staticData[0, 0].onSelect( %list, %text, true );
      }

      // Select the new tab:
      %tab = $pref::FavCurrentSelect - ( $pref::FavCurrentList * 10 ) + 1;
      InventoryScreen.selId = %tab;
      $Hud['inventoryScreen'].staticData[0, %tab].setValue( true );

      // Update the Edit Name field:
      $Hud['inventoryScreen'].staticData[1, 1].setValue( $pref::FavNames[%index] );
   }

   if ( %echo )
      addMessageHudLine( "Inventory set \"" @ $pref::FavNames[%index] @ "\" selected." );

   commandToServer( 'setClientFav', $pref::Favorite[%index] );
}

//------------------------------------------------------------------------------
function saveFavorite()
{
   if ( $pref::FavCurrentSelect !$= "" )
   {
      %favName = $Hud['inventoryScreen'].staticData[1, 1].getValue();
      $pref::FavNames[$pref::FavCurrentSelect] = %favName;
      $Hud['inventoryScreen'].staticData[0, $pref::FavCurrentSelect - ($pref::FavCurrentList * 10) + 1].setText( strupr( %favName ) );
      //$Hud[%tag].staticData[1, 1].setValue( %favName );
      %favList = $Hud['inventoryScreen'].data[0, 1].type TAB $Hud['inventoryScreen'].data[0, 1].getValue();
      for ( %i = 1; %i < $Hud['inventoryScreen'].count; %i++ )
      {
         %name = $Hud['inventoryScreen'].data[%i, 1].getValue();
         if ( %name $= invalid )
            %name = "EMPTY";
         %favList = %favList TAB $Hud['inventoryScreen'].data[%i, 1].type TAB %name;
      }
      $pref::Favorite[$pref::FavCurrentSelect] = %favList;
      echo("exporting pref::* to ClientPrefs.cs");
      export("$pref::*", "prefs/ClientPrefs.cs", False);
   }
//   else
//      addMessageHudLine("Must First Select A Favorite Button.");
}

//------------------------------------------------------------------------------
function addQuickPackFavorite( %pack, %item )
{
	// this has been such a success it has been changed to handle grenades
	// and other equipment as well as packs so everything seems to be called 'pack'
	// including the function itself. The default IS pack

	if(%item $= "")
		%item = "Pack";
	%packFailMsg = "You cannot use that equipment with your selected loadout.";
	if ( !isObject($Hud['inventoryScreen'].staticData[1, 1]) || $Hud['inventoryScreen'].staticData[1, 1].getValue() $= ""  )
	{
		//if the player hasnt brought up the inv screen we use his current fav
		%currentFav = $pref::Favorite[$pref::FavCurrentSelect];
		//echo(%currentFav);

		for ( %i = 0; %i < getFieldCount( %currentFav ); %i++ )
		{
			%type = getField( %currentFav, %i );
			%equipment = getField( %currentFav, %i++ );

			%invalidPack = checkPackValidity(%pack, %equipment, %item );
			if(%invalidPack)
			{
				addMessageHudLine( %packFailMsg );
				return;

			}
		// Success--------------------------------------------------
			if ( %type $= %item )
				%favList = %favList @ %type TAB %pack @ "\t";
			else
				%favList = %favList  @ %type TAB %equipment @ "\t";
		}
		//echo(%favList);
	}
	else
	{
		//otherwise we go with whats on the invScreen (even if its asleep)
		%armor =  $Hud['inventoryScreen'].data[0, 1].getValue();

		// check pack validity with armor
		%invalidPack = checkPackValidity(%pack, %armor, %item );
		if(%invalidPack)
		{
			addMessageHudLine( %packFailMsg );
			return;

		}
	   %favList = $Hud['inventoryScreen'].data[0, 1].type TAB %armor;
		for ( %i = 1; %i < $Hud['inventoryScreen'].count; %i++ )
		{
			//echo( $Hud['inventoryScreen'].Data[%i, 1].type);
			%type = $Hud['inventoryScreen'].data[%i, 1].type;
			%equipment = $Hud['inventoryScreen'].data[%i, 1].getValue();

			if(%type $= %item)
				%equipment = %pack;

		// Special Cases again------------------------------------------------
			%invalidPack = checkPackValidity(%pack, %equipment, %item );
			if(%invalidPack)
			{
				addMessageHudLine( %packFailMsg );
				return;

			}

			%favList = %favList TAB %type TAB %equipment;
		}
		//echo(%favList);
	}
	commandToServer( 'setClientFav', %favList );

	//we message the player real nice like
	addMessageHudLine( "Inventory updated to " @ %pack @ "." );
}

function checkPackValidity(%pack, %equipment, %item)
{
	//echo("validityChecking:" SPC %pack SPC %equipment);

	// this is mostly for ease of mod makers
	// this is the base restrictions stuff
	// for your mod just overwrite this function and
	// change the restrictions and onlyUses

	// you must have #1 to use #2
	//%restrict[#1, #2] = true;

	%restrict["Scout", "Inventory Station"] = true;
	%restrict["Scout", "Landspike Turret"] = true;
	%restrict["Scout", "Spider Clamp Turret"] = true;
	%restrict["Scout", "ELF Turret Barrel"] = true;
	%restrict["Scout", "Mortar Turret Barrel"] = true;
	%restrict["Scout", "AA Turret Barrel"] = true;
	%restrict["Scout", "Plasma Turret Barrel"] = true;
	%restrict["Scout", "Missile Turret Barrel"] = true;

	%restrict["Assault", "Cloak Pack"] = true;
	%restrict["Juggernaut", "Cloak Pack"] = true;

	// you can only use #1 if you have a #2 of type #3
	//%require[#1] = #2 TAB #3;

	%require["Laser Rifle"] = "Pack" TAB "Energy Pack";


 	if(%restrict[%equipment, %pack] )
 		return true;

	else if(%require[%equipment] !$="" )
	{
		if(%item $= getField(%require[%equipment], 0) )
		{
			if(%pack !$= getField(%require[%equipment], 1) )
				return true;
		}
	}
}


//------------------------------------------------------------------------------
function setDefaultInventory(%client)
{
   commandToClient(%client,'InitLoadClientFavorites');
}

//------------------------------------------------------------------------------
function checkInventory( %client, %text ) {
	%armor = getArmorDatablock( %client, $NameToInv[getField( %text, 1 )] );
	%list = getField( %text, 0 ) TAB getField( %text, 1 );
	%cmt = $CurrentMissionType;
	for( %i = 3; %i < getFieldCount( %text ); %i = %i + 2 ) {
		%inv = $NameToInv[getField(%text,%i)];
		if ( (( %armor.max[%inv] && !($InvBanList[%cmt, %inv]) ) ||
		getField( %text, %i ) $= Empty || getField( %text, %i ) $= Invalid)
		&& (($InvTotalCount[getField( %text, %i - 1 )] - $BanCount[getField( %text, %i - 1 )]) > 0))
			%list = %list TAB getField( %text, %i - 1 ) TAB getField( %text, %i );
		else if( $InvBanList[%cmt, %inv] || %inv $= empty || %inv $= "")
			%list = %list TAB getField( %text, %i - 1 ) TAB "INVALID";
	}
	return %list;
}

//------------------------------------------------------------------------------
function getArmorDatablock(%client, %size)
{
   if ( %client.race $= "Bioderm" )
      %armor = %size @ "Male" @ %client.race @ Armor;
   else
      %armor = %size @ %client.sex @ %client.race @ Armor;
   return %armor;
}

//------------------------------------------------------------------------------
function InventoryScreen::onWake(%this)
{
   if ( $HudHandle[inventoryScreen] !$= "" )
      alxStop( $HudHandle[inventoryScreen] );
   alxPlay(HudInventoryActivateSound, 0, 0, 0);
   $HudHandle[inventoryScreen] = alxPlay(HudInventoryHumSound, 0, 0, 0);

   if ( isObject( hudMap ) )
   {
      hudMap.pop();
      hudMap.delete();
   }
   new ActionMap( hudMap );
   hudMap.blockBind( moveMap, toggleScoreScreen );
   hudMap.blockBind( moveMap, toggleCommanderMap );
   hudMap.bindCmd( keyboard, escape, "", "InventoryScreen.onDone();" );
   hudMap.push();
}

//------------------------------------------------------------------------------
function InventoryScreen::onSleep()
{
   hudMap.pop();
   hudMap.delete();
   alxStop($HudHandle[inventoryScreen]);
   alxPlay(HudInventoryDeactivateSound, 0, 0, 0);
   $HudHandle[inventoryScreen] = "";
}

//------------------------------------------------------------------------------
function InventoryScreen::onDone( %this )
{
   toggleCursorHuds( 'inventoryScreen' );
}

//------------------------------------------------------------------------------
function InventoryScreen::onTabSelect( %this, %favId )
{
   loadFavorite( %favId, 0 );
}

function createInvBanCount()
{
   $BanCount["Armor"] = 0;
   $BanCount["Weapon"] = 0;
   $BanCount["Pack"] = 0;
   $BanCount["Dep"] = 0;
   $BanCount["Grenade"] = 0;
   $BanCount["Mine"] = 0;

   for(%i = 0; $InvArmor[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvArmor[%i]]])
         $BanCount["Armor"]++;
   $InvTotalCount["Armor"] = %i;

   for(%i = 0; $InvWeapon[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvWeapon[%i]]])
         $BanCount["Weapon"]++;
   $InvTotalCount["Weapon"] = %i;

   for(%i = 0; $InvPack[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvPack[%i]]])
         $BanCount["Pack"]++;
   $InvTotalCount["Pack"] = %i;

   for(%i = 0; $InvDep[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvDep[%i]]])
         $BanCount["Dep"]++;
   $InvTotalCount["Dep"] = %i;

   for(%i = 0; $InvGrenade[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvGrenade[%i]]])
         $BanCount["Grenade"]++;
   $InvTotalCount["Grenade"] = %i;

   for(%i = 0; $InvMine[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvMine[%i]]])
         $BanCount["Mine"]++;
   $InvTotalCount["Mine"] = %i;
}
