xquery version "3.1" encoding "utf-8";

declare namespace ar="http://autosar.org/schema/r4.0";
declare namespace xsd="http://www.w3.org/2001/XMLSchema";
declare namespace msdata="urn:schemas-microsoft-com:xml-msdata";
declare namespace main="main";

declare default element namespace "main";

declare variable $outdir external;
declare variable $resxFilePath external;

declare variable $db := 'XSD';
declare variable $main:restrTypes := <restrs>
  <restr type="AP"><pattern>AP</pattern></restr>
  <restr type="CP"><pattern>CP</pattern><pattern>Cp</pattern></restr>
  <restr type="FO"><pattern>FO</pattern></restr>
  <restr type="CP AP"><pattern>CP,AP</pattern><pattern>CP, AP</pattern></restr>
  <restr type="CP TC"><pattern>CP,TC</pattern></restr>
  <restr type="CP FO"><pattern>CP,FO</pattern><pattern>CP, FO</pattern></restr>
  <restr type="AP FO"><pattern>AP,FO</pattern><pattern>FO,AP</pattern></restr>
  <restr type="CP AP FO"><pattern>CP,AP,FO</pattern></restr>
  <restr type="CP AP TC"><pattern>AP, CP,TC</pattern></restr>
  <restr type="CP FO TC TA"><pattern>CP, FO, TC, TA</pattern></restr>
</restrs>;
declare variable $main:root := db:get($db);

declare function main:nodesWithRestr($root as document-node()+,$restrtype as xs:string) as element()* {
  (: let $r := trace($restrtype) :)
  let $nodes := (
    for $pattern in $main:restrTypes//restr[@type=$restrtype]/pattern
      (: let $r := trace($pattern/text()) :)
      let $searchPattern := concat('mmt.RestrictToStandards="',$pattern/text(),'"')
      (: let $r := trace($searchPattern) :)
      let $nodes := (
        for $node in $root//*:appinfo[contains(.,$searchPattern)]/../..
          let $nodeType := $node/local-name()
          order by $nodeType
          group by $nodeType
          return (
            <node category="{$nodeType}">
            {
                for $node1 in $node
                  let $name := $node1/@name
                  let $info :=                 
                    switch ($nodeType)
                    case ('enumeration') return <info name="{$node1/@value}"/>
                    case ('complexType') return <type name="{$node1/@name}"/>
                    case ('simpleType') return <type name="{$node1/@name}"/> 
                    case ('element') return <element name="{$node1/@name}"/>
                    case ('group') return if($node1[@name and ((*:sequence/*) or (*:choice/*) or (*:all/*)) ] ) then <info name="{$node1/@name}"/>                    
                    case ('attribute') return <attribute name="{$node1/@name}"/>
                    case ('attributeGroup') return <info name="{$node1/@name}"/>
                    default return <unknown/>
                  return ($info)
            }     
            </node>
          )        
      )
      let $nodeCategory := $nodes/*:node/@category
      order by $nodeCategory
      group by $nodeCategory
      return (
        for $node in $nodes
        return $node
      )
  )
  return (<nodes>{$nodes}</nodes>)  
};

declare function main:main() as element()* {
  for $root in $main:root
    let $xsd := fn:tokenize(fn:base-uri($root), '/')[last()]
    return <xsd id="{$xsd}">
    {
      for $restrType in $main:restrTypes//restr
        let $res :=
        <restriction type="{$restrType/@type}">
        {
          main:nodesWithRestr($root,$restrType/@type)
        }
        </restriction>
        return $res
    }
    </xsd>
};

declare function main:output($xsdA as element()*) {
  for $xsd in $xsdA
    let $outfile := concat($outdir, fn:tokenize($xsd/@id, '\.')[1], '.xml')
    let $dummy := trace(concat('Outfile = ', $outfile))
    let $res := $xsd
    return file:write($outfile, $res, map { 
      "method": "xml",
      "omit-xml-declaration": "no",
      "encoding": "UTF-8",
      "indent": "yes"
    })
};

declare function main:updateResx($xsdA as element()*) {
  if (file:exists($resxFilePath)) then
    let $resxDoc := doc($resxFilePath)
    
    let $newDataNodes := 
      for $xsd in $xsdA
        let $basename := fn:tokenize($xsd/@id, '\.')[1]
        (: Ersetze Bindestriche durch Unterstriche für den C# Variablennamen (AUTOSAR_4-0-3 -> AUTOSAR_4_0_3) :)
        let $safeName := replace($basename, '-', '_')
        return 
          <data name="{$safeName}" type="System.Resources.ResXFileRef, System.Windows.Forms" xmlns="">
            <value>.\input\{$basename}.xml;System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089;Windows-1252</value>
          </data>

    let $newRoot := 
      <root xmlns="">
        {
          for $child in $resxDoc/*:root/node()
          return 
            if (local-name($child) = 'data' and starts-with($child/@name, 'AUTOSAR_')) then 
              () 
            else if ($child instance of text() and normalize-space($child) = '') then
              ()
            else 
              $child
        }
        {
          (: Hänge die neu generierten Knoten an :)
          $newDataNodes
        }
      </root>
      
    let $dummy := trace("Aktualisiere Resources.resx")
    return file:write($resxFilePath, $newRoot, map { 
      "method": "xml",
      "omit-xml-declaration": "no",
      "encoding": "utf-8",
      "indent": "yes"
    })
  else
    trace(concat("Fehler: Resources.resx nicht gefunden unter ", $resxFilePath))
};

let $res := main:main()
let $dummy1 := main:output($res)
let $dummy2 := main:updateResx($res)
return ()