# NpcLogic
NPC base, health &amp; state logic written in C# for Unity

## NpcBase.cs
The foundation logic, could be split into further classes but currently holds expected properties/components, as well as logic for becoming alerted/alarmed 
(alerted means it's suspicous and will investigate where it last heard you, alarmed means it knows you're there and they're coming!)


## NpcHealth.cs
Health logic, handles npc UI healthbar & taking damage logic


## NpcStates > NpcStateManager.cs
The State Manager is a singleton class that holds an instance of all the Npc's states (idle, investigating, dead etc..) and runs the logic to switch between them.
It also has a reference to all of the npc's in the scene so it could be used to group npc's and stagger routines like walking paths for performance or create world events.
