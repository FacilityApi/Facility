#requires -PSEdition Core
#requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

function Get-CronParts {
	param(
		[Parameter(Mandatory = $true)]
		[string] $CronExpression
	)

	$parts = $CronExpression.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
	if ($parts.Count -lt 5) {
		return $null
	}

	return $parts
}

function Normalize-WorkflowCronMinutes {
	param(
		[Parameter(Mandatory = $true)]
		[string] $WorkflowContent
	)

	$pattern = '(?m)^(\s*(?:-\s*)?cron:\s*)(["'']?)([^"''\r\n]+)\2(\s*)$'
	return [System.Text.RegularExpressions.Regex]::Replace($WorkflowContent, $pattern, {
		param($match)

		$parts = Get-CronParts -CronExpression $match.Groups[3].Value
		if ($null -eq $parts) {
			return $match.Value
		}

		$parts[0] = '*'
		$cronExpression = $parts -join ' '
		return "{0}{1}{2}{1}{3}" -f $match.Groups[1].Value, $match.Groups[2].Value, $cronExpression, $match.Groups[4].Value
	})
}

function Randomize-ZeroWorkflowCronMinutes {
	param(
		[Parameter(Mandatory = $true)]
		[string] $WorkflowContent
	)

	$pattern = '(?m)^(\s*(?:-\s*)?cron:\s*)(["'']?)([^"''\r\n]+)\2(\s*)$'
	return [System.Text.RegularExpressions.Regex]::Replace($WorkflowContent, $pattern, {
		param($match)

		$parts = Get-CronParts -CronExpression $match.Groups[3].Value
		if (($null -eq $parts) -or ($parts[0] -ne '0')) {
			return $match.Value
		}

		$parts[0] = [string] (Get-Random -Minimum 1 -Maximum 60)
		$cronExpression = $parts -join ' '
		return "{0}{1}{2}{1}{3}" -f $match.Groups[1].Value, $match.Groups[2].Value, $cronExpression, $match.Groups[4].Value
	})
}

# Copy each published workflow template into the target repository when it differs.
$workflowNames = @(
	'apply-repo-conventions.yml',
	'ci.yml',
	'copilot-setup-steps.yml'
)

# Ensure the target workflows directory exists before copying templates.
$targetDirectory = Join-Path (Get-Location) '.github' 'workflows'
[System.IO.Directory]::CreateDirectory($targetDirectory) | Out-Null

# Compare and copy workflow templates using stable UTF-8 output.
foreach ($workflowName in $workflowNames) {
	$sourcePath = Join-Path $PSScriptRoot 'files' $workflowName
	$targetPath = Join-Path $targetDirectory $workflowName
	$sourceContent = [System.IO.File]::ReadAllText($sourcePath)
	$targetContent = if (Test-Path -LiteralPath $targetPath -PathType Leaf) { [System.IO.File]::ReadAllText($targetPath) } else { $null }
	$normalizedSourceContent = Normalize-WorkflowCronMinutes -WorkflowContent $sourceContent
	$normalizedTargetContent = if ($null -eq $targetContent) { $null } else { Normalize-WorkflowCronMinutes -WorkflowContent $targetContent }

	if ($normalizedSourceContent -cne $normalizedTargetContent) {
		$contentToWrite = Randomize-ZeroWorkflowCronMinutes -WorkflowContent $sourceContent
		[System.IO.File]::WriteAllText($targetPath, $contentToWrite, $utf8)
		Write-Host "Updated Facility workflow '.github/workflows/$workflowName'."
	}
}
