. .\services.ps1
$Script:PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$Script:Service:Name = 'Survey'
$Script:Service:Location = 'C:\Services\Survey'
$Script:Service:Build:Location = "\\data\Software\Wikiled\Survey"

function Install-App(
	[string]$server = $(throw "Please specify server"))
{
	write-host "##teamcity[message text='Processing $server deployment...']"							
	Stop-Service $Script:Service:Name $server
	Start-Sleep -s 1
	$destination = "\\$server\$Script:Service:Name"		
	write-host "##teamcity[message text='Deleting old $destination...']"			
	get-ChildItem -Path $destination -Recurse | Remove-Item -force -recurse		
		
	write-host "Copy [$Script:Service:Build:Location] to [$destination]..."		
	xcopy $Script:Service:Build:Location $destination\ /s /f /y
		
	Start-Service $Script:Service:Name $server
}		

function Install-Server([string]$server = $(throw "Please specify server"))
{
	write-host "Starting deployment to $server"						
	$currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name	
	Invoke-Command -ComputerName $server -scriptBlock ${function:New-Share} -ArgumentList $Script:Service:Location, $Script:Service:Name, $currentUser
	Install-App $server		
}
	
Function New-Share ($foldername, $sharename, $user) 
{
	# Test for existence of folder, if not there then create it 
	# 
	$shares=[WMICLASS]"WIN32_Share"
	if (!(Test-Path $foldername)) 
	{ 
		New-Item $foldername -type Directory 
	}

	# Create Share but check to make sure it isn’t already there 
	# 
	if (!(Get-WmiObject Win32_Share -filter "name='$Sharename'")) 
	{	
		$shares.Create($foldername, $sharename, 0) 
	}
	
	$acl = Get-WmiObject Win32_Share -filter "name='$Sharename'" | Get-Acl
	$ar = New-Object system.security.accesscontrol.filesystemaccessrule("$user", 'FullControl', 'ContainerInherit, ObjectInherit', 'None', 'Allow')
	$acl.AddAccessRule($ar)
	Set-Acl $foldername $acl
	
	write-host "##teamcity[message text='Granting ($sharename) share acces to ($user) access']"
	Grant-SmbShareAccess -Name $ShareName -AccountName $user -AccessRight Full -Force
}