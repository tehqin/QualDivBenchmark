for TRIAL in {0..19}
do
   dotnet bin/Release/netcoreapp2.1/publish/StrategySearch.dll config/sphere100L/sphere_cma_me_imp.tml $TRIAL &
#   dotnet bin/Release/netcoreapp2.1/publish/StrategySearch.dll config/rastrigin100L/rast_cma_me_rd.tml $TRIAL &
done
