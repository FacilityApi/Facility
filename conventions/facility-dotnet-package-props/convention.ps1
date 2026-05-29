#requires -PSEdition Core
#requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

# Configure the target MSBuild file and managed section markers.
$repositoryRoot = (Get-Location).Path
$targetRelativePath = 'Directory.Build.props'
$targetPath = Join-Path $repositoryRoot $targetRelativePath
$sectionStart = '<!-- DO NOT EDIT: facility-dotnet-package-props convention -->'
$sectionEnd = '<!-- END DO NOT EDIT -->'
$commonSectionStart = '<!-- DO NOT EDIT: dotnet-common-props convention -->'

# Create the target MSBuild file if the repository does not already have one.
if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) {
	[System.IO.File]::WriteAllText($targetPath, "<Project>`n</Project>`n", $utf8)
}

# Build the managed section from the repository directory name.
$repositoryName = Split-Path -Leaf $repositoryRoot
$managedSection = @"
  $sectionStart
  <PropertyGroup>
    <GitHubOrganization>FacilityApi</GitHubOrganization>
    <RepositoryName>$repositoryName</RepositoryName>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>
  $sectionEnd
"@.Replace("`r`n", "`n")
if (-not $managedSection.EndsWith("`n", [System.StringComparison]::Ordinal)) {
	$managedSection += "`n"
}

# Read the existing content and validate the target file shape.
$content = [System.IO.File]::ReadAllText($targetPath)
$normalizedContent = $content.Replace("`r`n", "`n")
if ($normalizedContent -notmatch '^\s*<Project[\s>]') {
	throw "Expected '$targetRelativePath' to be an MSBuild project file."
}

# Locate any existing managed section and fail on duplicate sections.
$startIndex = $normalizedContent.IndexOf($sectionStart, [System.StringComparison]::Ordinal)
$secondStartIndex = if ($startIndex -eq -1) { -1 } else { $normalizedContent.IndexOf($sectionStart, $startIndex + $sectionStart.Length, [System.StringComparison]::Ordinal) }
if ($secondStartIndex -ne -1) {
	throw "Found multiple '$sectionStart' sections in '$targetRelativePath'."
}

# Replace an existing managed section in place when present.
if ($startIndex -ne -1) {
	$lineStartIndex = $normalizedContent.LastIndexOf("`n", $startIndex, [System.StringComparison]::Ordinal)
	$lineStartIndex = if ($lineStartIndex -eq -1) { 0 } else { $lineStartIndex + 1 }
	$replacementStartIndex = if ($normalizedContent.Substring($lineStartIndex, $startIndex - $lineStartIndex).Trim().Length -eq 0) { $lineStartIndex } else { $startIndex }

	$endIndex = $normalizedContent.IndexOf($sectionEnd, $startIndex, [System.StringComparison]::Ordinal)
	if ($endIndex -eq -1) {
		throw "Found '$sectionStart' without '$sectionEnd' in '$targetRelativePath'."
	}

	$endIndex += $sectionEnd.Length
	if ($endIndex -lt $normalizedContent.Length -and $normalizedContent[$endIndex] -eq "`n") {
		$endIndex++
	}

	$newContent = $normalizedContent.Substring(0, $replacementStartIndex) + $managedSection + $normalizedContent.Substring($endIndex)
}
else {
	# Insert before the common props section when present, otherwise before the closing root element.
	$insertIndex = $normalizedContent.IndexOf("  $commonSectionStart", [System.StringComparison]::Ordinal)
	if ($insertIndex -eq -1) {
		$insertIndex = $normalizedContent.LastIndexOf('</Project>', [System.StringComparison]::Ordinal)
	}
	if ($insertIndex -eq -1) {
		throw "Expected '$targetRelativePath' to contain a closing Project element."
	}

	$newContent = $normalizedContent.Substring(0, $insertIndex) + $managedSection + $normalizedContent.Substring($insertIndex)
}

# Write the file only when the convention changed the content.
if ($newContent -cne $normalizedContent) {
	[System.IO.File]::WriteAllText($targetPath, $newContent, $utf8)
	Write-Host "Updated Facility package props in '$targetRelativePath'."
}
