#requires -PSEdition Core
#requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
$script:utf8 = $utf8
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

# Define the Pester suite for the Facility package props convention.
Describe 'facility-dotnet-package-props convention' {
	BeforeAll {
		# Record shared test state for direct convention invocation in isolated repositories.
		$script:utf8 = [System.Text.UTF8Encoding]::new($false)
		$script:conventionScriptPath = Join-Path $PSScriptRoot 'convention.ps1'

		# Create a temporary repository directory with a stable leaf name.
		function script:NewFacilityPropsTestDirectory {
			$parent = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString('N'))
			$path = Join-Path $parent 'ExampleRepo'
			[System.IO.Directory]::CreateDirectory($path) | Out-Null
			return $path
		}

		# Invoke the convention script from the specified temporary repository.
		function script:InvokeFacilityPropsConvention {
			param(
				[Parameter(Mandatory = $true)]
				[string] $TestDirectory
			)

			$inputPath = Join-Path $TestDirectory 'convention-input.json'
			[System.IO.File]::WriteAllText($inputPath, '{"settings":{}}', $script:utf8)
			try {
				Push-Location $TestDirectory
				try {
					& $script:conventionScriptPath $inputPath
				}
				finally {
					Pop-Location
				}
			}
			finally {
				Remove-Item -LiteralPath $inputPath -ErrorAction SilentlyContinue
			}
		}
	}

	It 'creates a managed section before dotnet-common-props and is idempotent' {
		# Set up an MSBuild props file with an existing common props section.
		$testDirectory = NewFacilityPropsTestDirectory

		try {
			$propsPath = Join-Path $testDirectory 'Directory.Build.props'
			[System.IO.File]::WriteAllText($propsPath, @'
<Project>
  <PropertyGroup>
    <VersionPrefix>1.2.3</VersionPrefix>
  </PropertyGroup>
  <!-- DO NOT EDIT: dotnet-common-props convention -->
  <PropertyGroup>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <!-- END DO NOT EDIT -->
</Project>
'@.Replace("`r`n", "`n"), $script:utf8)

			# Apply the convention and capture the generated content.
			InvokeFacilityPropsConvention -TestDirectory $testDirectory
			$content = Get-Content -LiteralPath $propsPath -Raw

			# Assert the managed Facility properties appear before common props and use the repository directory name.
			$content | Should -Match '<GitHubOrganization>FacilityApi</GitHubOrganization>'
			$content | Should -Match '<RepositoryName>ExampleRepo</RepositoryName>'
			$content | Should -Match '<PackageLicenseExpression>MIT</PackageLicenseExpression>'
			$content.IndexOf('facility-dotnet-package-props convention', [System.StringComparison]::Ordinal) | Should -BeLessThan $content.IndexOf('dotnet-common-props convention', [System.StringComparison]::Ordinal)
			$content | Should -Match "<!-- END DO NOT EDIT -->`n  <!-- DO NOT EDIT: dotnet-common-props convention -->"

			# Re-run the convention and assert the file is unchanged.
			InvokeFacilityPropsConvention -TestDirectory $testDirectory
			(Get-Content -LiteralPath $propsPath -Raw) | Should -Be $content
		}
		finally {
			# Remove the isolated repository after the test completes.
			Remove-Item -LiteralPath (Split-Path -Parent $testDirectory) -Recurse -Force
		}
	}

	It 'creates a minimal MSBuild props file when missing' {
		# Set up an empty repository with no Directory.Build.props file.
		$testDirectory = NewFacilityPropsTestDirectory

		try {
			# Apply the convention and assert a valid props file was created.
			InvokeFacilityPropsConvention -TestDirectory $testDirectory
			$content = Get-Content -LiteralPath (Join-Path $testDirectory 'Directory.Build.props') -Raw
			$content | Should -Match '^<Project>'
			$content | Should -Match '<RepositoryName>ExampleRepo</RepositoryName>'
			$content | Should -Match "<!-- END DO NOT EDIT -->`n</Project>"
			$content | Should -Match '</Project>'
		}
		finally {
			# Remove the isolated repository after the test completes.
			Remove-Item -LiteralPath (Split-Path -Parent $testDirectory) -Recurse -Force
		}
	}
}
