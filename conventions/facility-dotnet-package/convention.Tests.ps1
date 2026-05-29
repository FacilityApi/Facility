#requires -PSEdition Core
#requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

# Define the Pester suite for the Facility package composite convention.
Describe 'facility-dotnet-package convention' {
	It 'includes Facility props before common package conventions' {
		# Read the composite configuration so the expected child conventions can be checked.
		$content = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'convention.yml') -Raw

		# Assert the Facility props convention appears before the common props convention.
		$content.IndexOf('path: ../facility-dotnet-package-props', [System.StringComparison]::Ordinal) | Should -BeLessThan $content.IndexOf('path: Faithlife/CodingGuidelines/conventions/dotnet-common-props', [System.StringComparison]::Ordinal)
		$content | Should -Match 'Faithlife/CodingGuidelines/conventions/license-mit'
		$content | Should -Match 'copyright-holder: Ed Ball'
		$content | Should -Match 'message: Update Facility \.NET repository conventions'
	}
}
