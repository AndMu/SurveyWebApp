function Get-Service(
	    [string]$serviceName = $(throw "serviceName is required"), 
	    [string]$targetServer = $(throw "targetServer is required"))
{
	$service = Get-WmiObject -Namespace "root\cimv2" -Class "Win32_Service" `
		-ComputerName $targetServer -Filter "Name='$serviceName'" -Impersonation 3    
	return $service
}
	
function Test-ServiceResult(
    [string]$operation = $(throw "operation is required"), 
    [object]$result = $(throw "result is required"), 
    [switch]$continueOnError = $true)
{
    $retVal = -1
    if ($result.GetType().Name -eq "UInt32") 
	{ 
		$retVal = $result 
	} 
	else 
	{	
		$retVal = $result.ReturnValue
	}
        
    if ($retVal -eq 0) 
	{
		return
	}
    
    $errorcode = 'Success,Not Supported,Access Denied,Dependent Services Running,Invalid Service Control'
    $errorcode += ',Service Cannot Accept Control, Service Not Active, Service Request Timeout'
    $errorcode += ',Unknown Failure, Path Not Found, Service Already Running, Service Database Locked'
    $errorcode += ',Service Dependency Deleted, Service Dependency Failure, Service Disabled'
    $errorcode += ',Service Logon Failure, Service Marked for Deletion, Service No Thread'
    $errorcode += ',Status Circular Dependency, Status Duplicate Name, Status Invalid Name'
    $errorcode += ',Status Invalid Parameter, Status Invalid Service Account, Status Service Exists'
    $errorcode += ',Service Already Paused'
    $desc = $errorcode.Split(',')[$retVal]
    	
    $msg = ("##teamcity[message text='{0} failed with code {1}:{2}' status='" -f $operation, $retVal, $desc)
    
    if (!$continueOnError) 
	{
		write-host ("{0}ERROR']" -f $msg)
		[System.Environment]::Exit(1)
	} 
	else 
	{ 
		write-host ("{0}WARNING']" -f $msg)
	}        
}

function Stop-Service(
    [string]$serviceName = $(throw "serviceName is required"), 
    [string]$targetServer = $(throw "targetServer is required"))
{
	write-host "##teamcity[message text='Stopping service $serviceName on $targetServer']"
    $service = Get-Service $serviceName $targetServer
    
    if (!($service))
    { 
        write-host "##teamcity[message text='Failed to find service $serviceName on $targetServer. Nothing to uninstall.']"
        return
    }
    
    write-host "##teamcity[message text='Found service $serviceName on $targetServer; checking status']"
            
    if ($service.Started)
    {
        write-host "##teamcity[message text='Stopping service $serviceName on $targetServer"        
        $result = $service.StopService()
        Test-ServiceResult -operation "Stop service $serviceName on $targetServer" -result $result
    }    
}

function Start-Service(
    [string]$serviceName = $(throw "serviceName is required"), 
    [string]$targetServer = $(throw "targetServer is required"))
{
    write-host "##teamcity[message text='Getting service $serviceName on server $targetServer']"
    $service = Get-Service $serviceName $targetServer
    if (!($service.Started))
    {
        write-host "##teamcity[message text='Starting service $serviceName on server $targetServer']"
        $result = $service.StartService()
        Test-ServiceResult -operation "Starting service $serviceName on $targetServer" -result $result 
    }
}

